using MassTransit;
using Scalar.AspNetCore;
using TicketFlow.OrderingService.Domain.Commands;
using TicketFlow.OrderingService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");
builder.AddRedisClient("redis");

builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("ordering", false));

    x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq") ?? "rabbitmq");
        cfg.UseMessageRetry(r => r.Exponential(5,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(3)));
        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddScoped<ReserveOrderHandler>();
builder.Services.AddScoped<PayOrderHandler>();
builder.Services.AddScoped<CancelOrderHandler>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapDefaultEndpoints();
app.Run();
