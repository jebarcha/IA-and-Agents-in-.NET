namespace FirstChatbox.Services;

internal class WeatherServiceFake : IWeatherService
{
    public async Task<string> GetWeather(string city)
    {
        return city.ToLower() switch
        {
            "santo domingo" => "Sunny day, 32`C",
            "madrid" => "Cloudy day, 18`C",
            "new york" => "Raining, 12`C",
            _ => "I don't have the information for that city"
        };

    }
}
