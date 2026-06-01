using Microsoft.Extensions.AI;

namespace FirstChatbox.Chatboxs;

internal class Chatbot
{
    internal static async Task Run(IChatClient client)
    {
        Console.WriteLine("IA: Hi! You can write your questions or press ENTER to exit");
        Console.WriteLine();

        var chatHistory = new List<ChatMessage>();

        var systemPromptGeneral = """
            You are an assistant that responds general questions.
            You must respond in american english.
            The answers must be in plain-text, do not use formats such markdown.

            if a tool fail, read the error message in order to see if you can fix it making an adjustment. Communicate any fix you are going to make to the user.
            """;

        var systemPromptCsharp = """
            You are an expert in C# and .NET
            You must respond in american english and give examples.
            The answers must be in plain-text, do not use formats such as markdown.
            """;

        chatHistory.Add(new ChatMessage(role: ChatRole.System, systemPromptGeneral));

        while (true)
        {
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
            Console.Write($"IA: ");

            while (true)
            {
                var updates = new List<ChatResponseUpdate>();


                await foreach (var responseUpdate in client.GetStreamingResponseAsync(chatHistory))
                {
                    updates.Add(responseUpdate);

                    foreach (var content in responseUpdate.Contents)
                    {
                        if (content is TextContent textContent)
                        {
                            Console.Write(textContent);
                        }
                    }
                }

                var response = updates.ToChatResponse();
                chatHistory.AddMessages(response);

                var approvalRequest = response.Messages
                                    .SelectMany(m => m.Contents)
                                    .OfType<ToolApprovalRequestContent>()
                                    .FirstOrDefault();

                if (approvalRequest is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("IA wants to execute a sensitive action");

                    if (approvalRequest.ToolCall is FunctionCallContent functionCall)
                    {
                        Console.WriteLine($"Tool: {ConvertFunctionName(functionCall.Name)}");

                        if (functionCall.Arguments is not null)
                        {
                            foreach (var arg in functionCall.Arguments)
                            {
                                Console.WriteLine($"{arg.Key}: {arg.Value}");
                            }
                        }
                    }

                    Console.ResetColor();
                    Console.WriteLine("Do you want to approve this action? (y/n): ");
                    var approved = Console.ReadLine()?.Trim().ToLower() == "y";
                    var approvedResponse = approvalRequest.CreateResponse(approved);

                    chatHistory.Add(new ChatMessage(ChatRole.User, [approvedResponse]));

                    Console.WriteLine();
                    Console.Write("IA: ");
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine();
                break;
            }
        }
    }

    private static string ConvertFunctionName(string name)
    {
        return name switch
        {
            "SendEmail" => "Send email",
            _ => name
        };
    }
}
