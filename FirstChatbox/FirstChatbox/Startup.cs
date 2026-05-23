using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FirstChatbox;

internal static class Startup
{
    public static void ConfigureServices(HostApplicationBuilder builder, string provider, string? model)
    {
        var OpenAIKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
        var AnthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_KEY");

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
            })
            //.Use(async (messages, options, next, cancellationToken) =>
            //{
            //    Console.WriteLine();
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine("Before calling the model...");
            //    Console.ResetColor();

            //    await next(messages, options, cancellationToken);

            //    Console.WriteLine();
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine("After calling the model...");
            //    Console.ResetColor();
            //})
            .Build(sp);
            ;
        });
    }
}
