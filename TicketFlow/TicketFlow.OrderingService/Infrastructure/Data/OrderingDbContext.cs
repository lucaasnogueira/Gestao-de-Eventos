using MassTransit;
using Microsoft.EntityFrameworkCore;
using TicketFlow.OrderingService.Domain.Entities;

namespace TicketFlow.OrderingService.Infrastructure.Data;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order>     Orders     => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Order>().HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        mb.Entity<OrderItem>().Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");

        mb.AddInboxStateEntity();
        mb.AddOutboxMessageEntity();
        mb.AddOutboxStateEntity();
    }
}
