namespace MyAspireApp.ApiService;

public record WeatherForecastDto(DateOnly date, int temperatureC, string? summary)
{
    public int temperatureF => 32 + (int)(temperatureC / 0.5556);
}
