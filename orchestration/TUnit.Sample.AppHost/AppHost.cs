using TUnit.Sample.Common.Constants;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(ResourceConstants.Postgres)
    .WithDataVolume()
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Persistent);

var coreDb = postgres.AddDatabase(ResourceConstants.CoreDb);

var apiService = builder.AddProject<Projects.TUnit_Sample_ApiService>(ResourceConstants.WebApi)
    .WithHttpHealthCheck("/health")
    .WithReference(coreDb)
    .WaitFor(coreDb);

builder.AddProject<Projects.TUnit_Sample_Web>(ResourceConstants.WebFrontend)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();