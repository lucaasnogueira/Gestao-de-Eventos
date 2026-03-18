var builder = DistributedApplication.CreateBuilder(args);

// -- Infraestrutura --------------------------------------------------------
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var catalogDb  = postgres.AddDatabase("catalog-db");
var orderingDb = postgres.AddDatabase("ordering-db");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// -- Microservices ---------------------------------------------------------
var catalogService = builder
    .AddProject<Projects.TicketFlow_CatalogService>("catalog-service")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WithReference(rabbitMq);

var orderingService = builder
    .AddProject<Projects.TicketFlow_OrderingService>("ordering-service")
    .WithReference(orderingDb)
    .WithReference(redis)
    .WithReference(rabbitMq);

builder
    .AddProject<Projects.TicketFlow_WorkerService>("worker-service")
    .WithReference(orderingDb)
    .WithReference(redis)
    .WithReference(rabbitMq);

builder
    .AddProject<Projects.TicketFlow_ApiGateway>("api-gateway")
    .WithReference(catalogService)
    .WithReference(orderingService);

builder.Build().Run();
