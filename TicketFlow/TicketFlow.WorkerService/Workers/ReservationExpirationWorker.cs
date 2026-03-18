using MassTransit;
using StackExchange.Redis;
using System.Text.Json;
using TicketFlow.Contracts.Events;

namespace TicketFlow.WorkerService.Workers;

public class ReservationExpirationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessExpiredReservationsAsync(); }
            catch (Exception ex) { logger.LogError(ex, "Erro ao processar reservas expiradas"); }
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task ProcessExpiredReservationsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints().First());
        int expired = 0;

        await foreach (var key in server.KeysAsync(pattern: "reservation:*"))
        {
            var raw = await db.StringGetAsync(key);
            if (raw.IsNullOrEmpty) continue;
            var reservation = JsonSerializer.Deserialize<ReservationData>(raw!);
            if (reservation is null || reservation.ExpiresAt > DateTime.UtcNow) continue;

            await db.StringIncrementAsync($"stock:{reservation.TicketTypeId}", reservation.Quantity);
            await db.KeyDeleteAsync(key);

            await publisher.Publish(new OrderExpiredEvent
            {
                OrderId = reservation.OrderId, CustomerId = reservation.CustomerId, Reason = "Expired",
                Items = [new OrderItemSnapshot { TicketTypeId = reservation.TicketTypeId, Quantity = reservation.Quantity }]
            });

            logger.LogInformation("Reserva expirada: {OrderId}", reservation.OrderId);
            expired++;
        }
        if (expired > 0) logger.LogInformation("{Count} reservas expiradas.", expired);
    }

    private record ReservationData(Guid OrderId, Guid CustomerId, Guid TicketTypeId, int Quantity, DateTime ExpiresAt);
}
