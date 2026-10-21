using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Dtos;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Services;

public interface IReportService
{
    Task<SalesSummaryDto> GetSalesSummaryAsync(DateOnly startDate, DateOnly endDate);
}

public class ReportService(OrderFlowDbContext db) : IReportService
{
    public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new BusinessRuleException("startDate must be on or before endDate.");

        // LINQ over the SQL view. EF Core translates this into a single SQL query:
        // SELECT ... FROM vw_DailySales WHERE SaleDate BETWEEN ... GROUP BY ProductId, ProductName
        var products = await db.DailySales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => new { s.ProductId, s.ProductName })
            .OrderByDescending(g => g.Sum(s => s.Revenue))   // best-selling products first
            .Select(g => new ProductSalesDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(s => s.OrderCount),
                g.Sum(s => s.QuantitySold),
                g.Sum(s => s.Revenue)))
            .ToListAsync();

        return new SalesSummaryDto(
            startDate,
            endDate,
            products.Sum(p => p.QuantitySold),
            products.Sum(p => p.Revenue),
            products);
    }
}
