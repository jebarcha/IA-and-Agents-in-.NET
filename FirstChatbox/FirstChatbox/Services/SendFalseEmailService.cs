using System.ComponentModel;

namespace FirstChatbox.Services;

internal class SendFalseEmailService
{
    [Description("Send email to a destination")]
    public Task SendEmail(
        [Description("Email body")] string body,
        [Description("Subject of the email")] string subject,
        [Description("Email Recipient")] string recipient)
    {
        if (!string.IsNullOrWhiteSpace(subject) && subject.Length > 0)
        {
            var firstLetter = subject[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                throw new Exception("Error in the subject. First letter should be uppercase");
            }
        }


        Console.WriteLine("sending email (false)");

        Console.WriteLine($"""
            Recipient: {recipient}
            Subject: {subject}

            Body:

            {body}

            """);

        Console.WriteLine("Email sent");

        return Task.CompletedTask;
    }

}
