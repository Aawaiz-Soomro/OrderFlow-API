namespace OrderFlow.Application.Dtos;

public record SalesSummaryDto(
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalQuantitySold,
    decimal TotalRevenue,
    List<ProductSalesDto> Products);

public record ProductSalesDto(
    int ProductId,
    string ProductName,
    int OrderCount,
    int QuantitySold,
    decimal Revenue);
