namespace OrderFlow.Domain.Entities;

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }

    // Price is copied from the product at order time so later price changes don't affect old orders.
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
