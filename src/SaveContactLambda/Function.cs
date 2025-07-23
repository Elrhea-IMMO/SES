using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using SaveContactLambda.Models;
using System.Text.Json;

// Fix ambiguous class by aliasing the conflicting Message class from SES
using SESMessage = Amazon.SimpleEmail.Model.Message;

// Lambda function entry point
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SaveContactLambda;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDb = new AmazonDynamoDBClient();
    private readonly IAmazonSimpleEmailService _ses = new AmazonSimpleEmailServiceClient();
    private readonly IAmazonSQS _sqs = new AmazonSQSClient();

    private const string DynamoTableName = "ContactSubmissions";
    private const string VerifiedSenderEmail = "tudentgoa103@gmail.com";
    private const string AdminNotifyQueueUrl = "https://sqs.ap-south-1.amazonaws.com/your-account-id/admin-notify-queue";

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { message = "Invalid request body" })
            };
        }

        var submission = JsonSerializer.Deserialize<ContactSubmission>(request.Body!);
        if (submission == null || string.IsNullOrWhiteSpace(submission.Email))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { message = "Invalid contact submission" })
            };
        }

        submission.Timestamp = DateTime.UtcNow;

        // Save to DynamoDB
        var table = Table.LoadTable(_dynamoDb, DynamoTableName);
        var document = new Document
        {
            ["Email"] = submission.Email,
            ["Name"] = submission.Name,
            ["Age"] = submission.Age,
            ["Message"] = submission.Message,
            ["Timestamp"] = submission.Timestamp.ToString("o")
        };
        await table.PutItemAsync(document);

        // Send Welcome Email
        var welcomeEmail = new SendEmailRequest
        {
            Destination = new Destination { ToAddresses = new List<string> { submission.Email } },
            Message = new SESMessage
            {
                Subject = new Content("Thank you for contacting us!"),
                Body = new Body
                {
                    Text = new Content("We received your message and will respond shortly.")
                }
            },
            Source = VerifiedSenderEmail
        };
        await _ses.SendEmailAsync(welcomeEmail);

        // Send to SQS
        var adminMessage = new
        {
            submission.Name,
            submission.Email,
            submission.Timestamp
        };
        await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = AdminNotifyQueueUrl,
            MessageBody = JsonSerializer.Serialize(adminMessage)
        });

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = "Contact saved and welcome email sent." }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
