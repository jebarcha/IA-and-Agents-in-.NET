using FirstChatbox.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace FirstChatbox.Tools;

internal static class Tools
{
    internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var WeatherService = sp.GetRequiredService<IWeatherService>();

        yield return AIFunctionFactory.Create(
            WeatherService.GetWeather,
            new AIFunctionFactoryOptions
            {
                Name = "get_weather",
                Description = "Get the weather of the city specified"
            });

        var ConditionsEvaluatorService = sp.GetRequiredService<ConditionsEvaluatorService>();

        yield return AIFunctionFactory.Create(
            ConditionsEvaluatorService.EvaluateCondition,
            new AIFunctionFactoryOptions
            {
                Name = "evaluate_weather_condition",
                Description = "Evaluate a weather condition (for example: \"sunny\", \"light rain\", \"cloudy\") and determine whether it is a good time to do outdoor activities."
            });

        var getEmailService = sp.GetRequiredService<GetFalseEmailService>();
        yield return AIFunctionFactory.Create(getEmailService.GetEmail);

        var emailService = sp.GetRequiredService<SendFalseEmailService>();
        var sendEmailFunction = AIFunctionFactory.Create(emailService.SendEmail);
        yield return new ApprovalRequiredAIFunction(sendEmailFunction);
    }
}
