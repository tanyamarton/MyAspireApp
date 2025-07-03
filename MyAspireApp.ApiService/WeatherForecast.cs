using System.Text.Json.Serialization;

namespace MyAspireApp.ApiService;

public class WeatherForecast
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public string? Summary { get; set; }
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

	[JsonPropertyName("partitionKey")]
	public string PartitionKey { get; set; } = "forecast";

}
