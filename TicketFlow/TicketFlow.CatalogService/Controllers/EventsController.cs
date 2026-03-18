using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using TicketFlow.CatalogService.Domain.Entities;
using TicketFlow.CatalogService.Infrastructure.Data;

namespace TicketFlow.CatalogService.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(CatalogDbContext db, IConnectionMultiplexer redis) : ControllerBase
{
    private readonly IDatabase _redis = redis.GetDatabase();

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var events = await db.Events
            .Where(e => e.Status == EventStatus.Published)
            .Include(e => e.Venue).Include(e => e.TicketTypes)
            .OrderBy(e => e.StartsAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsNoTracking().ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var ev = await db.Events.Include(e => e.Venue).Include(e => e.TicketTypes)
            .AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> Availability(Guid id)
    {
        var ticketTypes = await db.TicketTypes.Where(t => t.EventId == id).AsNoTracking().ToListAsync();
        var availability = new Dictionary<Guid, long>();
        foreach (var tt in ticketTypes)
        {
            var stock = await _redis.StringGetAsync($"stock:{tt.Id}");
            availability[tt.Id] = stock.HasValue ? (long)stock : tt.AvailableQuantity;
        }
        return Ok(availability);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TicketFlow.Contracts.Dtos.CreateEventRequest request)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            VenueId = request.VenueId,
            ImageUrl = request.ImageUrl,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.Id }, ev);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TicketFlow.Contracts.Dtos.UpdateEventRequest request)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();

        if (Enum.TryParse<EventStatus>(request.Status, true, out var parsedStatus))
        {
            ev.Status = parsedStatus;
        }

        ev.Title = request.Title; 
        ev.Description = request.Description;
        ev.StartsAt = request.StartsAt; 
        ev.EndsAt = request.EndsAt;
        ev.ImageUrl = request.ImageUrl; 
        ev.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await _redis.KeyDeleteAsync($"event:{id}");
        return Ok(ev);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        ev.Status = EventStatus.Cancelled; ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await _redis.KeyDeleteAsync($"event:{id}");
        return NoContent();
    }
}
