namespace BlazorIA.Services
{
    internal interface IWeatherService
    {
        Task<string> GetWeather(string city);
    }
}