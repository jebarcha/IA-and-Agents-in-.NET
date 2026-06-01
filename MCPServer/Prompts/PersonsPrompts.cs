using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.Prompts
{
    [McpServerPromptType]
    public class PersonsPrompts
    {
        [McpServerPrompt, Description("Prompt to look for all persons")]
        public static ChatMessage GetAll() => new ChatMessage(ChatRole.User,
            """
            Get the full list of persons using the available tool.
            Then show the information in english language in clearly and briefly.
            """);

        [McpServerPrompt, Description("Prompt to search a person by  id.")]
        public static ChatMessage GetById(
        [Description("Id de la persona a consultar.")] int id)
        => new(
            ChatRole.User,
            $"""
            Search the person by id {id} using the available tool.

            If does exist:
            - show the data in english language,
            - show if is active or not.

            If does not exist:
            - tell it clearly.
            """
        );

        [McpServerPrompt, Description("Prompt to activate a person.")]
        public static ChatMessage ActivatePerson(
                [Description("Id of the person.")] int id)
                => new(
                    ChatRole.User,
                    $"""
                    Activate the person with id {id} using the available tool.
                    You must send isActive = true.

                    Then explain in english language if the operation was successfull or not.
                    """
                    );

        [McpServerPrompt, Description("Prompt to deactivate a person.")]
        public static ChatMessage DeactivatePerson(
                    [Description("Id of the person.")] int id)
                    => new(
                        ChatRole.User,
                        $"""
                        Desactivte the person with id {id} usaing the available tool.
                        You must send isActive = false.

                        Then explain in english language if the operation was successfull or not.
                        """
                    );
    }
}
