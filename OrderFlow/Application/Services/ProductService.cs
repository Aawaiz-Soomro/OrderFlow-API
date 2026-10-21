using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Dtos;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
}

public class ProductService(OrderFlowDbContext db) : IProductService
{
    public Task<List<ProductDto>> GetAllAsync() =>
        db.Products
          .AsNoTracking()
          .OrderBy(p => p.Name)
          .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.UnitPrice, p.StockQuantity))
          .ToListAsync();
}
