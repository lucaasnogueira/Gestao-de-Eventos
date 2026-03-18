namespace TicketFlow.Contracts.Dtos;

public record ReserveOrderRequest
{
    public Guid TicketTypeId { get; init; }
    public int  Quantity     { get; init; }
}

public record ReserveOrderResponse
{
    public Guid     OrderId   { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record PayOrderRequest
{
    public string PaymentMethodId { get; init; } = string.Empty;
    public string IdempotencyKey  { get; init; } = string.Empty;
}
