namespace TicketFlow.Contracts.Dtos;

public record CreateEventRequest
{
    public string   Title       { get; init; } = string.Empty;
    public string   Description { get; init; } = string.Empty;
    public DateTime StartsAt    { get; init; }
    public DateTime EndsAt      { get; init; }
    public Guid     VenueId     { get; init; }
    public string?  ImageUrl    { get; init; }
}

public record UpdateEventRequest
{
    public string   Title       { get; init; } = string.Empty;
    public string   Description { get; init; } = string.Empty;
    public DateTime StartsAt    { get; init; }
    public DateTime EndsAt      { get; init; }
    public string?  ImageUrl    { get; init; }
    public string   Status      { get; init; } = "Draft"; // "Draft", "Published", "Cancelled", "Completed"
}
