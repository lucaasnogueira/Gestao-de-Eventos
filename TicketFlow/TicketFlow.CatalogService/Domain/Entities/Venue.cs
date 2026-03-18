namespace TicketFlow.CatalogService.Domain.Entities;

public class Venue
{
    public Guid    Id          { get; set; } = Guid.NewGuid();
    public string  Name        { get; set; } = string.Empty;
    public string  Address     { get; set; } = string.Empty;
    public string  City        { get; set; } = string.Empty;
    public string  State       { get; set; } = string.Empty;
    public decimal Latitude    { get; set; }
    public decimal Longitude   { get; set; }
    public int     MaxCapacity { get; set; }
    public ICollection<Event> Events { get; set; } = [];
}
