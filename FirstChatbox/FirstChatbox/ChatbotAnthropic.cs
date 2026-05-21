
using Anthropic;
using Anthropic.Models.Messages;
using System.Text;
using System.Text.Json;

namespace FirstChatbox;

internal class ChatbotAnthropic
{
    internal static async Task Run()
    {
        var key = Environment.GetEnvironmentVariable("ANTHROPIC_KEY");
        var client = new AnthropicClient
        {
            ApiKey = key,
        };

        var model = "claude-haiku-4-5";

        Console.WriteLine("IA: Hi! You can write your questions or press ENTER to exit");
        Console.WriteLine();

        var chatHistory = new List<MessageParam>();

        var systemPromptCsharp = """
            You are an expert in C# and .NET
            You must respond in american english and give examples.
            The answers must be in plain-text, do not use formats such as markdown.
            """;

        while (true)
        {
            var sb = new StringBuilder();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                break;
            }

            chatHistory.Add(new MessageParam
            {
                Role = Role.User,
                Content = userInput
            });

            Console.WriteLine();
            Console.Write("IA: ");

            MessageCreateParams parameters = new()
            {
                Model = model,
                MaxTokens = 1024,
                System = systemPromptCsharp,
                Messages = chatHistory
            };

            await foreach (var message in client.Messages.CreateStreaming(parameters))
            {
                var text = ExtractTextDelta(message);

                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                    Console.Write(text);
                }
            }

            chatHistory.Add(new MessageParam
            {
                Role = Role.Assistant,
                Content = sb.ToString()
            });

            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private static string? ExtractTextDelta(object? update)
    {
        var json = update?.ToString();

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp) ||
                typeProp.GetString() != "content_block_delta")
            {
                return null;
            }

            if (!root.TryGetProperty("delta", out var deltaProp))
            {
                return null;
            }

            if (!root.TryGetProperty("type", out var deltaTypeProp) ||
               typeProp.GetString() != "text_delta")
            {
                return null;
            }

            if (!root.TryGetProperty("text", out var textProp))
            {
                return null;
            }

            return textProp.GetString();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
