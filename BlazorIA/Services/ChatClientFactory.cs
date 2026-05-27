using Anthropic;
using BlazorIA.Utils;
using Microsoft.Extensions.AI;

namespace BlazorIA.Services
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Create(string model)
        {
            var openAIKey = configuration.GetValue<string>("OPENAI_KEY");
            var anthropicKey = configuration.GetValue<string>("ANTHROPIC_KEY");

            var provider = AIModels.GetProvider(model);

            var client = provider switch
            {
                "openai" => new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", openAIKey).AsIChatClient(),
                "claude" => new AnthropicClient()
                {
                    ApiKey = anthropicKey
                }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = model ?? "claude-haiku-4-5").Build(),
                _ => throw new ArgumentException($"Unknown provider: {provider}")
            };

            return client.AsBuilder()
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
            .Build(sp);
        }
    }
}