using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using System.IO;
using VerifyOtpLambda;
using SaveContactLambda;
using AdminNotifyLambda;  // Add this for AdminNotifyLambda

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Verify OTP endpoint
app.MapPost("/verify", async (HttpContext context) =>
{
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var apiGatewayRequest = new APIGatewayProxyRequest
    {
        Body = requestBody
    };

    var lambda = new VerifyOtpLambda.Function(); // fully qualified to avoid ambiguity
    var result = await lambda.FunctionHandler(apiGatewayRequest, null);

    context.Response.StatusCode = result.StatusCode;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(result.Body);
});

// Save Contact endpoint
app.MapPost("/submit", async (HttpContext context) =>
{
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var apiGatewayRequest = new APIGatewayProxyRequest
    {
        Body = requestBody
    };

    var lambda = new SaveContactLambda.Function(); // fully qualified
    var result = await lambda.FunctionHandler(apiGatewayRequest, null);

    context.Response.StatusCode = result.StatusCode;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(result.Body);
});

// Test AdminNotifyLambda SQS handler endpoint
app.MapGet("/test-adminnotify", async (HttpContext context) =>
{
    var function = new AdminNotifyLambda.Function();

    var testEvent = new SQSEvent
    {
        Records = new List<SQSEvent.SQSMessage>
        {
            new SQSEvent.SQSMessage
            {
                Body = JsonSerializer.Serialize(new {
                    Name = "John Doe",
                    Email = "john@example.com",
                    Timestamp = System.DateTime.UtcNow.ToString("o")
                })
            }
        }
    };

    await function.FunctionHandler(testEvent, null);

    await context.Response.WriteAsync("AdminNotifyLambda test executed successfully.");
});

app.Run("http://localhost:5000");
