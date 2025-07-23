using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Text.Json;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Lambda function entry point
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace VerifyOtpLambda;

public class Function
{
    private readonly IAmazonSimpleEmailService _ses = new AmazonSimpleEmailServiceClient();
    private static readonly Dictionary<string, string> otpStore = new(); // Temp in-memory store (use Redis or DynamoDB for prod)

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Body!);
        string email = data["email"];
        string phone = data["phone"];

        var otp = new Random().Next(100000, 999999).ToString();
        otpStore[email] = otp;

        var sendRequest = new SendEmailRequest
        {
            Destination = new Destination { ToAddresses = new List<string> { email } },
            Message = new Message
            {
                Subject = new Content("Your OTP for Contact Form"),
                Body = new Body
                {
                    Text = new Content($"Your OTP is: {otp}")
                }
            },
            Source = "studentgoa103@gmail.com"
        };

        await _ses.SendEmailAsync(sendRequest);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = "OTP sent to email" }),
        };
    }
}
