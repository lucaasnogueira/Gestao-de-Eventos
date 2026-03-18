using StackExchange.Redis;
using System.Text.Json;
using TicketFlow.Contracts.Dtos;
using TicketFlow.OrderingService.Domain.Entities;
using TicketFlow.OrderingService.Infrastructure.Data;
using TicketFlow.OrderingService.Infrastructure.Redis;

namespace TicketFlow.OrderingService.Domain.Commands;

public class ReserveOrderHandler(OrderingDbContext db, IConnectionMultiplexer redis, ILogger<ReserveOrderHandler> logger)
{
    private readonly IDatabase _redis = redis.GetDatabase();

    public async Task<ReserveOrderResponse> HandleAsync(ReserveOrderRequest request, Guid customerId, string customerEmail, CancellationToken ct = default)
    {
        // Lua Script: Checks if stock is sufficient and if so, decrements it atomically.
        // Returns 1 if successful, 0 if insufficient stock.
        const string luaScript = @"
            local stock = tonumber(redis.call('GET', KEYS[1]))
            if stock == nil then return 0 end
            local requested = tonumber(ARGV[1])
            if stock >= requested then
                redis.call('DECRBY', KEYS[1], requested)
                return 1
            else
                return 0
            end
        ";

        var stockKey = $"stock:{request.TicketTypeId}";
        
        var result = await _redis.ScriptEvaluateAsync(luaScript, [stockKey], [request.Quantity]);
        if ((int)result == 0) throw new InvalidOperationException("Estoque insuficiente ou indisponível.");

        var order = new TicketFlow.OrderingService.Domain.Entities.Order { CustomerId = customerId, CustomerEmail = customerEmail, Status = OrderStatus.Reserved };
        order.SetExpiration(TimeSpan.FromMinutes(10));
        db.Orders.Add(order);

        var reservationData = new ReservationData
        {
            OrderId = order.Id, CustomerId = customerId,
            TicketTypeId = request.TicketTypeId, Quantity = request.Quantity,
            ExpiresAt = order.ExpiresAt!.Value
        };

        var reservationKey = $"reservation:{order.Id}";
        await _redis.StringSetAsync(reservationKey, JsonSerializer.Serialize(reservationData), TimeSpan.FromMinutes(10));
        
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reserva criada: {OrderId} | Expira: {ExpiresAt}", order.Id, order.ExpiresAt);
        return new ReserveOrderResponse { OrderId = order.Id, ExpiresAt = order.ExpiresAt!.Value };
    }
}
