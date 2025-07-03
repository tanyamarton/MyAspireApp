using Microsoft.Azure.Cosmos;
using MyAspireApp.ApiService;
using Microsoft.AspNetCore.Mvc;

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
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/weatherforecast", async (HttpContext context, [FromBody] WeatherForecastDto dto) =>
{
    try
    {
        var cosmosClient = context.RequestServices.GetRequiredService<CosmosClient>();

        const string databaseId = "WeatherDb";
        const string containerId = "Forecasts";

        var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        var db = dbResponse.Database;
        await db.CreateContainerIfNotExistsAsync(containerId, "/PartitionKey");

        var container = db.GetContainer("Forecasts");

        var forecast = new WeatherForecast
        {
            Id = Guid.NewGuid().ToString(),
            Date = dto.Date.ToDateTime(TimeOnly.MinValue),
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary,
            PartitionKey = "forecast"
        };

        await container.CreateItemAsync(forecast, new PartitionKey(forecast.PartitionKey));
        return Results.Created($"/weatherforecast/{forecast.Id}", forecast);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Exception thrown", detail: ex.ToString());
    }
})
.WithName("CreateWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record FakeWeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
