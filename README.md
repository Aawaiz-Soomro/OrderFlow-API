# OrderFlow API

A Sales Order Management backend built with **C#, ASP.NET Core Web API (.NET 10), Entity Framework Core, LINQ, SQL Server and Swagger**.

Features:
- Customers, products, orders and order lines in a relational SQL Server schema (EF Core migrations + seed data)
- **Transactional order creation**: checks stock, deducts stock and calculates totals on the server, then saves everything at once. If anything fails, it all rolls back.
- **Order status lifecycle** `Created → Confirmed → Shipped → Invoiced`, forward-only, one step at a time
- **Date-range sales report** that queries a **SQL View** through EF Core and LINQ

---

## Project structure

One project, split into folders by layer so it's easy to follow:

```
OrderFlow/
├── Controllers/            API layer: thin HTTP endpoints, no business logic
├── Middleware/             API layer: GlobalExceptionHandler (exceptions -> HTTP status codes)
├── Application/
│   ├── Dtos/               Request/response records (entities are never exposed)
│   └── Services/           Business logic (OrderService, ReportService, ...)
├── Domain/
│   ├── Entities/           Customer, Product, Order, OrderLine, OrderStatus, DailySales (view)
│   └── Exceptions/         BusinessRuleException (400), NotFoundException (404)
├── Infrastructure/
│   ├── Data/               OrderFlowDbContext (EF mapping) + SeedData
│   └── Migrations/         InitialCreate (tables + seed), AddDailySalesView (SQL view)
└── Program.cs              Dependency injection + HTTP pipeline + Swagger
```

How a request flows: **Controller → Service → DbContext → SQL Server**, and the response comes back as a **DTO**.

---

## Run in the browser with GitHub Codespaces (nothing to install)

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/Aawaiz-Soomro/OrderFlow-API)

1. Click the badge above (or on GitHub: **Code → Codespaces → Create codespace on main**).
2. Wait a few minutes the first time. The dev container (`.devcontainer/`) starts SQL Server 2022 in a second container, restores packages, runs `dotnet ef database update`, and starts the API.
3. When port **5152** opens in a new tab, add **`/swagger`** to the end of the URL.

If the tab doesn't open, go to the **Ports** tab, click the globe icon next to 5152, and add `/swagger`. To restart the API, open a terminal and run `cd OrderFlow && dotnet run --launch-profile codespaces`.

To let someone else (e.g. an interviewer) open it, right-click port 5152 in the **Ports** tab and choose **Port Visibility → Public**. The codespace stops on its own after 30 minutes of inactivity. Reopen it from github.com/codespaces.

---

## Run locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server. Either one works:
  - **LocalDB** (comes with Visual Studio on Windows). This is the default connection string.
  - **Docker**: `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Pass123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`
- EF Core CLI tool: run `dotnet tool restore` in the repo root (installs the version pinned in `dotnet-tools.json`)

If you use Docker or another server, change the connection string in `OrderFlow/appsettings.json`, for example:

```json
"OrderFlowDb": "Server=localhost,1433;Database=OrderFlowDb;User Id=sa;Password=Your_strong_Pass123;TrustServerCertificate=True"
```

### Apply the database migrations

```bash
cd OrderFlow
dotnet ef database update
```

This creates the `OrderFlowDb` database and applies both migrations:
1. `InitialCreate`: creates the `Customers`, `Products`, `Orders` and `OrderLines` tables and seeds 3 customers and 5 products
2. `AddDailySalesView`: creates the `vw_DailySales` SQL view

(If you'd rather run plain SQL in SSMS, `dotnet ef migrations script -o migrate.sql` generates the full script.)

### Run and open Swagger

```bash
cd OrderFlow
dotnet run --launch-profile https
```

Then open **https://localhost:7155/swagger** (or http://localhost:5152/swagger). Visual Studio / Rider open it automatically.

---

## Testing the endpoints in Swagger

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers` | List seeded customers |
| GET | `/api/products` | List products and current stock |
| GET | `/api/orders` | List orders |
| GET | `/api/orders/{id}` | Get one order with its lines |
| POST | `/api/orders` | Create an order (transactional) |
| PATCH | `/api/orders/{id}/status` | Move an order to the next status |
| GET | `/api/reports/sales?startDate=&endDate=` | Sales summary from the SQL view |

**Seed data:** Customers 1–3 (Acme, Globex, Initech). Products: 1 Laptop ($1200, stock 10), 2 Monitor ($300, 25), 3 Keyboard ($80, 50), 4 Mouse ($25, 100), 5 Docking Station ($150, 5).

### 1. Create a valid order → `201 Created`
`POST /api/orders`
```json
{ "customerId": 1, "lines": [ { "productId": 1, "quantity": 2 }, { "productId": 4, "quantity": 3 } ] }
```
The total is calculated on the server: 2 × 1200 + 3 × 25 = **2475**. Call `GET /api/products` and you'll see the Laptop stock is now 8 and the Mouse stock is 97.

### 2. Prove the rollback → `400 Bad Request`
```json
{ "customerId": 2, "lines": [ { "productId": 2, "quantity": 1 }, { "productId": 5, "quantity": 99 } ] }
```
The second line fails (`Insufficient stock for 'USB-C Docking Station'`). Call `GET /api/products` again: the Monitor stock is **still 25**, and `GET /api/orders` shows no new order. Nothing was partially saved. An unknown product (e.g. `productId: 999`) behaves the same way.

### 3. Status transitions
`PATCH /api/orders/1/status`
- `{ "status": "Invoiced" }` → **400** `Invalid status transition from 'Created' to 'Invoiced'.`
- `{ "status": "Confirmed" }` → 200, then `"Shipped"` → 200, then `"Invoiced"` → 200
- Going backwards (e.g. `"Created"` after `"Confirmed"`) → 400

### 4. Sales report
`GET /api/reports/sales?startDate=2026-01-01&endDate=2026-12-31` (use a range that includes today)

Returns total revenue, total quantity, and a per-product breakdown sorted by revenue. If `startDate` is after `endDate`, you get a 400.

---

## Key design points (interview notes)

**Layered architecture + DI.** Controllers only handle HTTP. Business rules live in services and entities. EF Core lives in Infrastructure. `Program.cs` registers the `DbContext` and services with the built-in DI container as **Scoped**, which means one instance per HTTP request. Controllers depend on interfaces (`IOrderService`), so they're easy to test and swap.

**DTOs.** Requests and responses are C# `record`s. The client never sends prices or totals and never receives EF entities. That avoids over-posting, circular JSON references, and leaking the database shape.

**Transaction (`OrderService.CreateAsync`).**
1. `BeginTransactionAsync()`
2. For each line: load the product, check stock, deduct stock, and calculate `LineTotal` from the product's current price
3. Calculate `TotalAmount`, then `SaveChangesAsync()` (inserts the order and lines and updates stock)
4. `CommitAsync()`. Any exception goes to `RollbackAsync()` and is rethrown.

`SaveChanges` is already atomic on its own. The explicit transaction makes the unit of work obvious, and it still holds if the method later grows to several `SaveChanges` calls.

**Concurrency.** `Product.RowVersion` is a SQL Server `rowversion` concurrency token. If two orders deduct the same product's stock at the same moment, the second save throws `DbUpdateConcurrencyException`, which returns **409 Conflict** instead of silently overselling.

**Status rules on the entity.** `Order.ChangeStatus()` only allows `newStatus == Status + 1`. Enum values 0–3 define the order of the lifecycle. The rule lives in the domain, so no caller can bypass it.

**SQL View + LINQ.** The migration `AddDailySalesView` creates `vw_DailySales` with raw SQL (sales grouped per day and per product). `DailySales` is mapped with `HasNoKey().ToView("vw_DailySales")`, so EF reads from it but never creates a table for it. `ReportService` filters and groups it with LINQ, and EF turns that into a single query:

```sql
SELECT ProductId, ProductName, SUM(OrderCount), SUM(QuantitySold), SUM(Revenue)
FROM vw_DailySales
WHERE SaleDate >= @start AND SaleDate <= @end
GROUP BY ProductId, ProductName
ORDER BY SUM(Revenue) DESC
```

**Error handling.** Services throw `NotFoundException` / `BusinessRuleException`. `GlobalExceptionHandler` (`IExceptionHandler`) turns them into ProblemDetails JSON responses (404 / 400 / 409 / 500), so controllers need no try/catch. Bad input (e.g. `quantity: 0`) is rejected with a 400 by `[ApiController]` model validation before reaching the service.
