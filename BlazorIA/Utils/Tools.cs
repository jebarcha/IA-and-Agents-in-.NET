using BlazorIA.Servicios;
using Microsoft.Extensions.AI;

namespace BlazorIA.Utils;

internal static class Tools
{
    internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var WeatherService = sp.GetRequiredService<IServicioClima>();

        yield return AIFunctionFactory.Create(
            WeatherService.ObtenerClima,
            new AIFunctionFactoryOptions
            {
                Name = "get_weather",
                Description = "Get the weather of the city specified"
            });

        var ConditionsEvaluatorService = sp.GetRequiredService<ServicioEvaluaCondiciones>();

        yield return AIFunctionFactory.Create(
            ConditionsEvaluatorService.EvaluarCondiciones,
            new AIFunctionFactoryOptions
            {
                Name = "evaluate_weather_condition",
                Description = "Evaluate a weather condition (for example: \"sunny\", \"light rain\", \"cloudy\") and determine whether it is a good time to do outdoor activities."
            });

        var getEmailService = sp.GetRequiredService<ServicioObtenerCorreoFalso>();
        yield return AIFunctionFactory.Create(getEmailService.ObtenerCorreo);

        var emailService = sp.GetRequiredService<ServicioEnviarCorreoFalso>();
        var sendEmailFunction = AIFunctionFactory.Create(emailService.EnviarCorreo);
        yield return new ApprovalRequiredAIFunction(sendEmailFunction);

        var servicioPersonas = sp.GetRequiredService<IServicioPersonas>();
        yield return AIFunctionFactory.Create(servicioPersonas.ObtenerTodas);
    }
}
