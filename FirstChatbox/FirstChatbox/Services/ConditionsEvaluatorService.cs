namespace FirstChatbox.Services;

internal class ConditionsEvaluatorService
{
    public string EvaluateCondition(string weatherCondition)
    {
        weatherCondition = weatherCondition.Trim().ToLower();

        // Rain / Drizzle / Precipitation
        if (weatherCondition.Contains("rain") ||
            weatherCondition.Contains("drizzle") ||
            weatherCondition.Contains("precipitation"))
            return "It's not a good time for outdoor activities";

        // Storms
        if (weatherCondition.Contains("storm") ||
            weatherCondition.Contains("stormy"))
            return "Avoid going outside, dangerous weather conditions";

        // Snow / Snowfall / Blizzard
        if (weatherCondition.Contains("snow") ||
            weatherCondition.Contains("snowfall") ||
            weatherCondition.Contains("blizzard"))
            return "Cold and potentially dangerous conditions, go out only if necessary";

        // Mist / Fog
        if (weatherCondition.Contains("mist") ||
            weatherCondition.Contains("fog"))
            return "Be careful when going out, visibility may be reduced";

        // Sunny
        if (weatherCondition.Contains("sunny"))
            return "Excellent weather to go outside";

        // Cloudy
        if (weatherCondition.Contains("cloudy"))
            return "You can go outside, but it's not ideal weather";

        return "Normal conditions";
    }
}
