using MassTransit;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Contracts.Events;
using TicketFlow.OrderingService.Domain.Entities;
using TicketFlow.OrderingService.Infrastructure.Data;

namespace TicketFlow.OrderingService.Domain.Commands;

public class CancelOrderHandler(OrderingDbContext db, IPublishEndpoint publishEndpoint, ILogger<CancelOrderHandler> logger)
{
    public async Task<bool> HandleAsync(Guid orderId, Guid customerId, string reason, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId, ct);
        if (order is null) return false;
        
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Expired)
        {
            return true; // Already cancelled or expired.
        }

        order.Status = OrderStatus.Cancelled;
        
        await publishEndpoint.Publish(new OrderCancelledEvent
        {
            OrderId = order.Id, CustomerId = order.CustomerId, Reason = reason,
            Items = order.Items.Select(i => new OrderItemSnapshot
            {
                TicketTypeId = i.TicketTypeId, EventId = i.EventId,
                Quantity = i.Quantity, UnitPrice = i.UnitPrice
            }).ToList()
        }, ct);
        
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} cancelled for reason {Reason}", orderId, reason);
        return true;
    }
}
