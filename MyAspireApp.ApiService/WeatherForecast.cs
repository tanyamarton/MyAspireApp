using System.Text.Json.Serialization;

namespace MyAspireApp.ApiService;

public class WeatherForecast
{
	
	public string id { get; set; } = Guid.NewGuid().ToString();
	public DateTime datetime { get; set; }
	public int temperatureC { get; set; }
	public string? summary { get; set; }
	public int temperatureF => 32 + (int)(temperatureC / 0.5556);

	
	public string partitionKey { get; set; } = "forecast";

}
