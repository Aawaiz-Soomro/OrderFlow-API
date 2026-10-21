using System.ComponentModel.DataAnnotations;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Dtos;

// ----- Requests -----

// The client only sends WHAT it wants; prices and totals are always calculated on the server.
public record CreateOrderRequest(
    [Range(1, int.MaxValue)] int CustomerId,
    [Required, MinLength(1)] List<CreateOrderLineRequest> Lines);

public record CreateOrderLineRequest(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(1, int.MaxValue)] int Quantity);

public record UpdateOrderStatusRequest([Required] OrderStatus Status);

// ----- Responses -----

public record OrderDto(
    int Id,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderLineDto> Lines);

public record OrderLineDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
