namespace TicketFlow.OrderingService.Domain.Entities;

public enum OrderStatus { Pending, Reserved, PaymentProcessing, Confirmed, Cancelled, Expired }

public class Order
{
    public Guid        Id              { get; set; } = Guid.NewGuid();
    public Guid        CustomerId      { get; set; }
    public string      CustomerEmail   { get; set; } = string.Empty;
    public OrderStatus Status          { get; set; } = OrderStatus.Pending;
    public decimal     TotalAmount     { get; set; }
    public DateTime    CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime?   ConfirmedAt     { get; set; }
    public DateTime?   ExpiresAt       { get; set; }
    public string?     PaymentIntentId { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public void SetExpiration(TimeSpan duration) => ExpiresAt = CreatedAt.Add(duration);
}
