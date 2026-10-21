namespace OrderFlow.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }

    // Concurrency token: if two requests deduct stock for the same product at the same time,
    // the second SaveChanges fails instead of silently overwriting the first one.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
