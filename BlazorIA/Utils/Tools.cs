using BlazorIA.Services;
using Microsoft.Extensions.AI;

namespace BlazorIA.Utils;

internal static class Tools
{
    internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var weatherService = sp.GetRequiredService<IWeatherService>();

        yield return AIFunctionFactory.Create(
            weatherService.GetWeather,
            new AIFunctionFactoryOptions
            {
                Name = "get_weather",
                Description = "Get the weather of the city specified"
            });

        var conditionsEvaluatorService = sp.GetRequiredService<ConditionEvaluatorService>();

        yield return AIFunctionFactory.Create(
            conditionsEvaluatorService.EvaluateConditions,
            new AIFunctionFactoryOptions
            {
                Name = "evaluate_weather_condition",
                Description = "Evaluate a weather condition (for example: \"sunny\", \"light rain\", \"cloudy\") and determine whether it is a good time to do outdoor activities."
            });

        var getEmailService = sp.GetRequiredService<FakeGetEmailService>();
        yield return AIFunctionFactory.Create(getEmailService.GetEmail);

        var emailService = sp.GetRequiredService<FakeSendEmailService>();
        var sendEmailFunction = AIFunctionFactory.Create(emailService.SendEmail);
        yield return new ApprovalRequiredAIFunction(sendEmailFunction);

        var personService = sp.GetRequiredService<IPersonService>();
        yield return AIFunctionFactory.Create(personService.GetAll);
    }
}
