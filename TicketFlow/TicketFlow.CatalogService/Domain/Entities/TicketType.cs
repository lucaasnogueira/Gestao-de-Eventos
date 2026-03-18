namespace TicketFlow.CatalogService.Domain.Entities;

public class TicketType
{
    public Guid     Id                { get; set; } = Guid.NewGuid();
    public Guid     EventId           { get; set; }
    public string   Name              { get; set; } = string.Empty;
    public string   Description       { get; set; } = string.Empty;
    public decimal  Price             { get; set; }
    public int      TotalCapacity     { get; set; }
    public int      AvailableQuantity { get; set; }
    public DateTime SaleStartsAt      { get; set; }
    public DateTime SaleEndsAt        { get; set; }
    public Event    Event             { get; set; } = null!;
}
