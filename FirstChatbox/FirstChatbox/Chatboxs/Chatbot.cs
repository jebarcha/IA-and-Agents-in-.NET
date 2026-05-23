using Microsoft.Extensions.AI;
using System.Text;

namespace FirstChatbox.Chatboxs;

internal class Chatbot
{
    internal static async Task Run(IChatClient client)
    {
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
            Console.Write($"IA:");

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
