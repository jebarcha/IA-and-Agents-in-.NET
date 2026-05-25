using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FirstChatbox.Services;

internal class OpenWeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<string> GetWeather(string city)
    {
        var apiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY");
        var cityUrl = Uri.EscapeDataString(city);
        //var url = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityUrl}&aqi=no&lang=es"; // with the language es or en, etc
        var url = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityUrl}&aqi=no";
        var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
        return weatherResponse!.Current.Condition.Text;
    }

    public class WeatherResponse
    {
        [JsonPropertyName("current")]
        public Current Current { get; set; } = default!;
    }
    public class Current
    {
        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = default!;
    }
    public class Condition
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }
}
