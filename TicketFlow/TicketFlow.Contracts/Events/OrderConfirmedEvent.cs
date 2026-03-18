namespace TicketFlow.Contracts.Events;

public record OrderConfirmedEvent
{
    public Guid OrderId         { get; init; }
    public Guid CustomerId      { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public DateTime ConfirmedAt { get; init; }
    public IReadOnlyList<OrderItemSnapshot> Items { get; init; } = [];
}

public record OrderItemSnapshot
{
    public Guid    TicketTypeId { get; init; }
    public Guid    EventId      { get; init; }
    public int     Quantity     { get; init; }
    public decimal UnitPrice    { get; init; }
}
