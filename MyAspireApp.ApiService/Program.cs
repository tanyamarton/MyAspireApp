using Microsoft.Azure.Cosmos;
using MyAspireApp.ApiService;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureCosmosClient("cosmos-db");

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new FakeWeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

//a get to /weatherforecast that returns all the weather forecasts in the database  
app.MapGet("/weatherforecast/list", async (HttpContext context) =>
{
    try
    {
        var cosmosClient = context.RequestServices.GetRequiredService<CosmosClient>();
        const string databaseId = "WeatherDb";
        const string containerId = "Forecasts";
        var container = cosmosClient.GetContainer(databaseId, containerId);
        var query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
            .WithParameter("@partitionKey", "forecast");
        var iterator = container.GetItemQueryIterator<WeatherForecast>(query);
        var results = new List<WeatherForecast>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return Results.Ok(results);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Exception thrown", detail: ex.ToString());
    }
})
.WithName("ListWeatherForecasts");

//get the weather forecast for a specific forecast id
app.MapGet("/weatherforecast/{id}", async (HttpContext context, string id) =>
{
    try
    {
        var cosmosClient = context.RequestServices.GetRequiredService<CosmosClient>();
        const string databaseId = "WeatherDb";
        const string containerId = "Forecasts";
        var container = cosmosClient.GetContainer(databaseId, containerId);
        var response = await container.ReadItemAsync<WeatherForecast>(id, new PartitionKey("forecast"));
        return Results.Ok(response.Resource);
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Exception thrown", detail: ex.ToString());
    }
})
.WithName("GetWeatherForecastById");

app.MapPost("/weatherforecast", async (HttpContext context, [FromBody] WeatherForecastDto dto) =>
{
    try
    {
        var cosmosClient = context.RequestServices.GetRequiredService<CosmosClient>();

        const string databaseId = "WeatherDb";
        const string containerId = "Forecasts";


        var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        var db = dbResponse.Database;
        await db.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");

        var container = db.GetContainer("Forecasts");

        var pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var nowPacific = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificTimeZone);

        var forecast = new WeatherForecast
        {
            id = Guid.NewGuid().ToString(),
            datetime = dto.datetime ?? nowPacific,
            temperatureC = dto.temperatureC,
            summary = dto.summary,
            partitionKey = "forecast"
        };

        await container.CreateItemAsync(forecast, new PartitionKey(forecast.partitionKey));
        return Results.Created($"/weatherforecast/{forecast.id}", forecast);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Exception thrown", detail: ex.ToString());
    }
})
.WithName("CreateWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record FakeWeatherForecast(DateTime datetime, int temperatureC, string? summary)
{
    public int temperatureF => 32 + (int)(temperatureC / 0.5556);
}

