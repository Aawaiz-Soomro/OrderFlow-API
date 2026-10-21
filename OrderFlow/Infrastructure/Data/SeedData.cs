using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data;

// Sample data inserted by the InitialCreate migration so the API can be tested immediately.
public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Acme Corporation", Email = "orders@acme.com" },
            new Customer { Id = 2, Name = "Globex Ltd", Email = "purchasing@globex.com" },
            new Customer { Id = 3, Name = "Initech", Email = "buyer@initech.com" });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Sku = "LAP-001", Name = "Laptop", UnitPrice = 1200.00m, StockQuantity = 10 },
            new Product { Id = 2, Sku = "MON-001", Name = "27\" Monitor", UnitPrice = 300.00m, StockQuantity = 25 },
            new Product { Id = 3, Sku = "KEY-001", Name = "Mechanical Keyboard", UnitPrice = 80.00m, StockQuantity = 50 },
            new Product { Id = 4, Sku = "MOU-001", Name = "Wireless Mouse", UnitPrice = 25.00m, StockQuantity = 100 },
            new Product { Id = 5, Sku = "DOC-001", Name = "USB-C Docking Station", UnitPrice = 150.00m, StockQuantity = 5 });
    }
}
