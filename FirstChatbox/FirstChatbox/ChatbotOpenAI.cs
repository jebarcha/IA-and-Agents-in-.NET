using OpenAI.Chat;
using System.Text;

namespace FirstChatbox;

internal class ChatbotOpenAI
{
    internal static async Task Run()
    {
        var model = "gpt-5.4-nano-2026-03-17";
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
        var client = new ChatClient(model, key);

        Console.WriteLine("IA: Hi! You can write your questions or press ENTER to exit");
        Console.WriteLine();

        var chatHistory = new List<ChatMessage>();

        var systemPromptGeneral = """
            You are an assitant that responds general questions.
            You must respond in american english.
            The answers must be in plain-text, do not use formats such markdown.
            """;

        var systemPromptCsharp = """
            You are an expert in C# and .NET
            You must respond in american english and give examples.
            The answers must be in plain-text, do not use formats such as markdown.
            """;

        var systemPromptPython = """
            You are an expert in Python
            You must respond in american english and give examples.
            The answers must be in plain-text, do not use formats such as markdown.
            """;

        chatHistory.Add(new SystemChatMessage(systemPromptCsharp));

        while (true)
        {
            var sb = new StringBuilder();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                break;
            }

            chatHistory.Add(new UserChatMessage(userInput));

            Console.WriteLine();
            Console.Write($"{model} IA:");

            var stream = client.CompleteChatStreamingAsync(chatHistory);

            await foreach (var message in stream)
            {
                foreach (var content in message.ContentUpdate)
                {
                    sb.Append(content.Text);
                    Console.Write(content.Text);
                }
            }

            chatHistory.Add(new AssistantChatMessage(sb.ToString()));

            Console.WriteLine();
        }
    }
}
