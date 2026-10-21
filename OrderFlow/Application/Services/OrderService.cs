using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Dtos;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(CreateOrderRequest request);
    Task<OrderDto> UpdateStatusAsync(int id, OrderStatus newStatus);
}

public class OrderService(OrderFlowDbContext db) : IOrderService
{
    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await OrdersWithDetails()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(int id)
    {
        var order = await OrdersWithDetails().FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Order {id} was not found.");

        return ToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request)
    {
        // Everything below is one unit of work: either the whole order is saved and all stock
        // is deducted, or nothing is. Any exception rolls the transaction back.
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId))
                throw new NotFoundException($"Customer {request.CustomerId} was not found.");

            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Created
            };

            foreach (var lineRequest in request.Lines)
            {
                // FindAsync returns the already-tracked product if it was loaded for an earlier line,
                // so the same product requested twice is checked against the already-reduced stock.
                var product = await db.Products.FindAsync(lineRequest.ProductId)
                    ?? throw new BusinessRuleException($"Product {lineRequest.ProductId} does not exist.");

                if (product.StockQuantity < lineRequest.Quantity)
                    throw new BusinessRuleException(
                        $"Insufficient stock for '{product.Name}'. Requested {lineRequest.Quantity}, available {product.StockQuantity}.");

                product.StockQuantity -= lineRequest.Quantity;

                order.Lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    Quantity = lineRequest.Quantity,
                    UnitPrice = product.UnitPrice,
                    LineTotal = product.UnitPrice * lineRequest.Quantity
                });
            }

            order.TotalAmount = order.Lines.Sum(l => l.LineTotal);

            db.Orders.Add(order);
            await db.SaveChangesAsync();      // INSERT order + lines, UPDATE product stock
            await transaction.CommitAsync();

            return await GetByIdAsync(order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;                            // let the global exception handler turn it into an HTTP response
        }
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatus newStatus)
    {
        var order = await db.Orders.FindAsync(id)
            ?? throw new NotFoundException($"Order {id} was not found.");

        order.ChangeStatus(newStatus);        // throws BusinessRuleException on an invalid transition
        await db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private IQueryable<Order> OrdersWithDetails() =>
        db.Orders
          .AsNoTracking()
          .Include(o => o.Customer)
          .Include(o => o.Lines)
              .ThenInclude(l => l.Product);

    // Entity -> DTO mapping, so entities are never returned to the client.
    private static OrderDto ToDto(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Customer.Name,
        order.OrderDate,
        order.Status.ToString(),
        order.TotalAmount,
        order.Lines
             .Select(l => new OrderLineDto(l.ProductId, l.Product.Name, l.Quantity, l.UnitPrice, l.LineTotal))
             .ToList());
}
