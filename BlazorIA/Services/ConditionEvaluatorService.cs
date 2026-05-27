using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorIA.Services;

internal class ConditionEvaluatorService
{
    public string EvaluateConditions(string weatherCondition)
    {
        weatherCondition = weatherCondition.ToLower();

        // Rain / Drizzle / Precipitation
        if (weatherCondition.Contains("rain") ||
            weatherCondition.Contains("drizzle") ||
            weatherCondition.Contains("precipitation"))
            return "Not a good time for outdoor activities";

        // Storms
        if (weatherCondition.Contains("storm") ||
            weatherCondition.Contains("stormy"))
            return "Avoid going out, dangerous weather conditions";

        // Snow / Snowfall / Blizzard
        if (weatherCondition.Contains("snow") ||
            weatherCondition.Contains("snowfall") ||
            weatherCondition.Contains("blizzard"))
            return "Cold and potentially dangerous conditions, go out only if necessary";

        // Haze / Fog
        if (weatherCondition.Contains("haze") ||
            weatherCondition.Contains("fog"))
            return "Be cautious when going out, visibility may be reduced";

        // Sunny
        if (weatherCondition.Contains("sunny"))
            return "Excellent weather to go out";

        // Cloudy
        if (weatherCondition.Contains("cloudy"))
            return "You can go out, but the weather is not ideal";

        return "Normal conditions";

    }
}
