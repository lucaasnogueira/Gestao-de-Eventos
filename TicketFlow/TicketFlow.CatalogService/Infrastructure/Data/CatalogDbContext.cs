using MassTransit;
using Microsoft.EntityFrameworkCore;
using TicketFlow.CatalogService.Domain.Entities;
using CatalogEvent = TicketFlow.CatalogService.Domain.Entities.Event;

namespace TicketFlow.CatalogService.Infrastructure.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<CatalogEvent> Events      => Set<CatalogEvent>();
    public DbSet<Venue>      Venues      => Set<Venue>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<TicketType>().Property(t => t.Price).HasColumnType("numeric(18,2)");
        mb.Entity<CatalogEvent>().HasOne(e => e.Venue).WithMany(v => v.Events).HasForeignKey(e => e.VenueId);
        mb.Entity<CatalogEvent>().HasMany(e => e.TicketTypes).WithOne(t => t.Event).HasForeignKey(t => t.EventId);

        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
