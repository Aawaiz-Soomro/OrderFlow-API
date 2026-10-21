namespace OrderFlow.Application.Dtos;

public record ProductDto(int Id, string Sku, string Name, decimal UnitPrice, int StockQuantity);
