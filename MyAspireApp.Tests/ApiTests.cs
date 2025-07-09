using System.Net.Http.Json;


namespace MyAspireApp.Tests;

[Trait("Type", "External")]
public class ApiTests
{

    private readonly HttpClient httpClient;

    public ApiTests()
    {
        // Use the port where apiservice is listening in the running Aspire AppHost
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7577") // replace with actual port from Aspire dashboard
        };

    }

    [Fact]
    public async Task GetWeatherForecastReturnsOkStatusCode()
    {

        // Act
        var response = await httpClient.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostWeatherForecastReturnsOkStatusCode()
    {
        // Arrange        
        var forecast = new
        {
            datetime = DateTime.UtcNow,
            temperatureC = 20,
            summary = "Sunny"
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync("/weatherforecast", forecast);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
