var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres");

var database = postgres
    .AddDatabase("minute-db");

var dbManager = builder
    .AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithHttpEndpoint(port: 5270, name: "http")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpCommand("/db/reset-seed", "Reset & Seed DB");

var keycloak = builder.AddKeycloak("keycloak")
    .WithEndpoint(port: 8080, targetPort: 8080, name: "http", scheme: "http")
    .WithDataVolume()
    .WithRealmImport("./keycloak");

var webApi = builder
    .AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi")
    .WithHttpEndpoint(port: 5555, name: "http")
    .WithReference(database)
    .WithReference(keycloak)
    .WaitFor(database)
    .WaitFor(keycloak);

var adminClient = builder.AddProject<Projects.UTB_Minute_AdminClient>("utb-minute-adminclient")
    .WithReference(webApi)
    .WithReference(keycloak)
    .WaitFor(webApi);

var canteenClient = builder.AddProject<Projects.UTB_Minute_CanteenClient>("utb-minute-canteenclient")
    .WithReference(webApi)
    .WithReference(keycloak)
    .WaitFor(webApi);

builder.Build().Run();
