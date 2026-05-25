using System.ComponentModel;

namespace FirstChatbox.Services;

internal class GetFalseEmailService
{
    [Description("Get the email of a person")]
    public string GetEmail([Description("The name of the person")] string name) => $"{name}@example.com";



}
