namespace TicketFlow.OrderingService.Infrastructure.Redis;

public record ReservationData
{
    public Guid     OrderId      { get; init; }
    public Guid     CustomerId   { get; init; }
    public Guid     EventId      { get; init; }
    public Guid     TicketTypeId { get; init; }
    public int      Quantity     { get; init; }
    public DateTime ExpiresAt    { get; init; }
    public string   Status       { get; init; } = "Pending";
}
