namespace MyAspireApp.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecastDto[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecastDto>? forecasts = null;

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecastDto>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }
}

public record WeatherForecastDto(DateTime datetime, int temperatureC, string? summary)
{
    public int temperatureF => 32 + (int)(temperatureC / 0.5556);
}
