namespace OrderFlow.Domain.Entities;

// The numeric values define the order of the lifecycle: each status can only move to the next one.
public enum OrderStatus
{
    Created = 0,
    Confirmed = 1,
    Shipped = 2,
    Invoiced = 3
}
