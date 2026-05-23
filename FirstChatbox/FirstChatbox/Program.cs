using FirstChatbox;
using FirstChatbox.Chatboxs;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Utils.LoadEnvironmentVars();

// dotnet run -- openai gpt-5.4-nano
// dotnet run -- claude claude-haiku-4-5

var provider = args.Length > 0 ? args[0].ToLowerInvariant() : "claude";
var defaultModel = provider == "openai" ? "gpt-5.4-nano" : "claude-haiku-4-5";
var model = args.Length > 1 ? args[1] : defaultModel;

Console.WriteLine($"{provider}: {model}");

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, provider, model);
var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
await Chatbot.Run(chatClient);
