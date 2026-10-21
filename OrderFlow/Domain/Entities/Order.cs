using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public decimal TotalAmount { get; set; }

    // One order has many order lines.
    public List<OrderLine> Lines { get; set; } = new();

    // Business rule lives on the entity: only forward, one-step transitions are allowed
    // (Created -> Confirmed -> Shipped -> Invoiced).
    public void ChangeStatus(OrderStatus newStatus)
    {
        if (newStatus != Status + 1)
        {
            throw new BusinessRuleException(
                $"Invalid status transition from '{Status}' to '{newStatus}'.");
        }

        Status = newStatus;
    }
}
