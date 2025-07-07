#pragma warning disable ASPIRECOSMOSDB001
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos-db")
        .RunAsPreviewEmulator(
        emulator =>
        {
            emulator.WithDataExplorer();
        });

var db = cosmos.AddCosmosDatabase("WeatherDb");
var container = db.AddContainer("Forecasts", "/partitionKey");

var cache = builder.AddRedis("cache");

// add .ApiService
var apiService = builder.AddProject<Projects.MyAspireApp_ApiService>("apiservice")
    .WithReference(cache)
    .WithReference(cosmos); 


// the cache and apiService objects that are needed for the build can be created by the builder

// add .Web
builder.AddProject<Projects.MyAspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(container);

// don't build until the cache and apiService resources are available
builder.Build().Run();
