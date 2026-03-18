using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TicketFlow.CatalogService.Infrastructure.Data;
using TicketFlow.Contracts.Events;

namespace TicketFlow.CatalogService.Infrastructure.Messaging;

public class OrderConfirmedConsumer(CatalogDbContext db, IConnectionMultiplexer redis, ILogger<OrderConfirmedConsumer> logger) : IConsumer<OrderConfirmedEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var msg = context.Message;
        var redisDb = redis.GetDatabase();
        foreach (var item in msg.Items)
        {
            var rows = await db.Database.ExecuteSqlRawAsync(
                "UPDATE ticket_types SET available_quantity = available_quantity - {0} WHERE id = {1} AND available_quantity >= {0}",
                item.Quantity, item.TicketTypeId);
            if (rows == 0) logger.LogWarning("Estoque insuficiente para {Id}", item.TicketTypeId);
            await redisDb.KeyDeleteAsync($"event:{item.EventId}");
        }
        await db.SaveChangesAsync();
    }
}

public class OrderCancelledConsumer(CatalogDbContext db, IConnectionMultiplexer redis) : IConsumer<OrderCancelledEvent>
{
    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var msg = context.Message;
        var redisDb = redis.GetDatabase();
        foreach (var item in msg.Items)
        {
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE ticket_types SET available_quantity = available_quantity + {0} WHERE id = {1}",
                item.Quantity, item.TicketTypeId);
            await redisDb.StringIncrementAsync($"stock:{item.TicketTypeId}", item.Quantity);
        }
        await db.SaveChangesAsync();
    }
}
