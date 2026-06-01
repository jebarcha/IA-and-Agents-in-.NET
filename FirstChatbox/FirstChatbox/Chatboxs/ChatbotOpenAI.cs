using Microsoft.Extensions.AI;
using System.Text;

namespace FirstChatbox.Chatboxs;

internal class ChatbotOpenAI
{
    internal static async Task Run()
    {
        var model = "gpt-5.4-nano";
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
        var client = new OpenAI.Chat.ChatClient(model, key).AsIChatClient();

        Console.WriteLine("IA: Hi! You can write your questions or press ENTER to exit");
        Console.WriteLine();

        var chatHistory = new List<ChatMessage>();

        var systemPromptGeneral = """
            You are an assistant that responds general questions.
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

        chatHistory.Add(new ChatMessage(role: ChatRole.System, systemPromptCsharp));

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

            chatHistory.Add(new ChatMessage(role: ChatRole.User, userInput));

            Console.WriteLine();
            Console.Write($"{model} IA: ");

            await foreach (var fragment in client.GetStreamingResponseAsync(chatHistory))
            {
                sb.Append(fragment.Text);
                Console.Write(fragment.Text);
            }

            chatHistory.Add(new ChatMessage(role: ChatRole.Assistant, sb.ToString()));

            Console.WriteLine();
        }
    }
}
