namespace OrderFlow.Domain.Entities;

// Read-only, keyless entity mapped to the SQL view "vw_DailySales" (one row per day per product).
public class DailySales
{
    public DateOnly SaleDate { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
