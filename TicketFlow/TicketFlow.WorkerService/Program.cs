using MassTransit;
using TicketFlow.WorkerService.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq") ?? "rabbitmq");
        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddHostedService<ReservationExpirationWorker>();

var host = builder.Build();
host.Run();
