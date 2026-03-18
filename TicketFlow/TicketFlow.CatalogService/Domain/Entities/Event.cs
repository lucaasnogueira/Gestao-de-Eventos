namespace TicketFlow.CatalogService.Domain.Entities;

public enum EventStatus { Draft, Published, Cancelled, Completed }

public class Event
{
    public Guid        Id          { get; set; } = Guid.NewGuid();
    public string      Title       { get; set; } = string.Empty;
    public string      Description { get; set; } = string.Empty;
    public DateTime    StartsAt    { get; set; }
    public DateTime    EndsAt      { get; set; }
    public Guid        VenueId     { get; set; }
    public Venue       Venue       { get; set; } = null!;
    public string?     ImageUrl    { get; set; }
    public EventStatus Status      { get; set; } = EventStatus.Draft;
    public DateTime    CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime    UpdatedAt   { get; set; } = DateTime.UtcNow;
    public ICollection<TicketType> TicketTypes { get; set; } = [];
}
