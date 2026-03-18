using MassTransit;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Contracts.Dtos;
using TicketFlow.Contracts.Events;
using TicketFlow.OrderingService.Domain.Entities;
using TicketFlow.OrderingService.Infrastructure.Data;

namespace TicketFlow.OrderingService.Domain.Commands;

public class PayOrderHandler(OrderingDbContext db, IPublishEndpoint publishEndpoint, ILogger<PayOrderHandler> logger)
{
    public async Task<Order?> HandleAsync(Guid orderId, PayOrderRequest request, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null) return null;

        // Idempotency check:
        if (order.PaymentIntentId == request.IdempotencyKey && order.Status == OrderStatus.Confirmed)
        {
            logger.LogInformation("Payment already processed for order {OrderId}", orderId);
            return order; // Already paid.
        }

        if (order.Status != OrderStatus.Reserved) throw new InvalidOperationException("Pedido não está reservado.");
        if (order.ExpiresAt < DateTime.UtcNow) throw new InvalidOperationException("Reserva expirada.");

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        order.PaymentIntentId = request.IdempotencyKey;
        
        await publishEndpoint.Publish(new OrderConfirmedEvent
        {
            OrderId = order.Id, CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail, ConfirmedAt = order.ConfirmedAt.Value,
            Items = order.Items.Select(i => new OrderItemSnapshot
            {
                TicketTypeId = i.TicketTypeId, EventId = i.EventId,
                Quantity = i.Quantity, UnitPrice = i.UnitPrice
            }).ToList()
        }, ct);
        
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} confirmed.", orderId);
        return order;
    }
}
