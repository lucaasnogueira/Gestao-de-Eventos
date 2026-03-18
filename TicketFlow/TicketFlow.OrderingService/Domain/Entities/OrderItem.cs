namespace TicketFlow.OrderingService.Domain.Entities;

public class OrderItem
{
    public Guid    Id             { get; set; } = Guid.NewGuid();
    public Guid    OrderId        { get; set; }
    public Guid    TicketTypeId   { get; set; }
    public Guid    EventId        { get; set; }
    public string  EventTitle     { get; set; } = string.Empty;
    public string  TicketTypeName { get; set; } = string.Empty;
    public decimal UnitPrice      { get; set; }
    public int     Quantity       { get; set; }
    public string? QrCodeHash     { get; set; }
    public Order   Order          { get; set; } = null!;
}
