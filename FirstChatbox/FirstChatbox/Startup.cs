using Anthropic;
using FirstChatbox.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FirstChatbox;

internal static class Startup
{
    public static void ConfigureServices(HostApplicationBuilder builder, string provider, string? model)
    {
        var OpenAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
        var AnthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_KEY");

        //builder.Services.AddTransient<IWeatherService, WeatherServiceFake>();
        builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
        builder.Services.AddTransient<ConditionsEvaluatorService>();
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
        builder.Services.AddHttpClient();

        builder.Services.AddTransient<SendFalseEmailService>();
        builder.Services.AddTransient<GetFalseEmailService>();

        builder.Services.AddSingleton<IChatClient>(sp =>
        {
            var client = provider switch
            {
                "openai" => new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", AnthropicKey).AsIChatClient(),
                "claude" => new AnthropicClient()
                {
                    ApiKey = AnthropicKey
                }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = model ?? "claude-haiku-4-5").Build(),
                _ => throw new ArgumentException($"Unknown provider {provider}")
            };

            return client.AsBuilder()
            .ConfigureOptions(o =>
            {
                o.MaxOutputTokens = 2000;
                o.Temperature = 0.7f;
                o.Tools = [.. Tools.Tools.GetTools(sp)];
            })
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
                .Use(async (messages, options, next, cancellationToken) =>
                {
                    await next(messages, options, cancellationToken);
                })
            .Build(sp);
            ;
        });
    }
}
