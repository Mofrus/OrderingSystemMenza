var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin();

var database = postgres
    .AddDatabase("minute_db");

var dbManager = builder
    .AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WaitFor(database);

var webApi = builder
    .AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
