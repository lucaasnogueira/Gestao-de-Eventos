namespace TicketFlow.Contracts.Events;

public record OrderCancelledEvent
{
    public Guid OrderId    { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyList<OrderItemSnapshot> Items { get; init; } = [];
    public string Reason   { get; init; } = string.Empty;
}

public record OrderExpiredEvent : OrderCancelledEvent { }
