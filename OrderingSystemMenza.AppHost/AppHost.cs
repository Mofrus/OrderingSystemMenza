var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres
    .AddDatabase("postgresdb");

var dbManager = builder
    .AddProject<Projects.OrderingSystemMenza_DbManager>("ordering-system-menza-dbmanager")
    .WithReference(database)
    .WaitFor(database);

var webApi = builder
    .AddProject<Projects.OrderingSystemMenza_Server>("ordering-system-menza-webapi")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();