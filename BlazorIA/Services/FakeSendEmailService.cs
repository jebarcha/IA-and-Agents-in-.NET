using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BlazorIA.Services;

internal class FakeSendEmailService
{
    [Description("Send an email to a recipient.")]
    public Task SendEmail(
        [Description("Email body")] string body,
        [Description("Email subject")] string subject,
        [Description("Recipient email")] string recipient)
    {

        if (!string.IsNullOrWhiteSpace(subject) && subject.Length > 0)
        {
            var firstLetter = subject[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                throw new Exception("Error with email subject. The first letter must be uppercase");
            }

        }

        Console.WriteLine("Sending email...");

        Console.WriteLine($"""

            Recipient: {recipient}
            Subject: {subject}

            Body:

            {body}

            """);

        Console.WriteLine("Email sent...");

        return Task.CompletedTask;
    }
}
