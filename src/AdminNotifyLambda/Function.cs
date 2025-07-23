using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

// Required for AWS Lambda to find this method.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AdminNotifyLambda;

public class Function
{
    private readonly IAmazonSimpleEmailService _ses = new AmazonSimpleEmailServiceClient();
    private const string AdminEmail = "studentgoa103@gmail.com";
    private const string SenderEmail = "studentgoa103@gmail.com";

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            var contact = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Body);
            var name = contact.GetValueOrDefault("Name", "Unknown");
            var email = contact.GetValueOrDefault("Email", "Unknown");
            var timestamp = contact.GetValueOrDefault("Timestamp", DateTime.UtcNow.ToString());

            var body = $"New contact form submission:\n\nName: {name}\nEmail: {email}\nTime: {timestamp}";

            var request = new SendEmailRequest
            {
                Source = SenderEmail,
                Destination = new Destination { ToAddresses = new List<string> { AdminEmail } },
                Message = new Message
                {
                    Subject = new Content("?? New Contact Form Submission"),
                    Body = new Body { Text = new Content(body) }
                }
            };

            await _ses.SendEmailAsync(request);
        }
    }
}
