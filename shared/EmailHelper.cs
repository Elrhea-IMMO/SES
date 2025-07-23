using Amazon.SimpleEmail;
using Amazon;

public class Function
{
    private readonly EmailHelper _emailHelper;

    public Function()
    {
        // Create SES client (make sure region is correct)
        var sesClient = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSouth1);

        // Use a verified sender email
        string senderEmail = "iitstudent500@gmail.com";

        _emailHelper = new EmailHelper(sesClient, senderEmail);
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Body);
        string recipientEmail = data["email"];

        string otp = OtpGenerator.GenerateOtp();

        string subject = "Your OTP Code";
        string bodyHtml = $"<p>Your OTP is: <strong>{otp}</strong></p>";
        string bodyText = $"Your OTP is: {otp}";

        await _emailHelper.SendEmailAsync(recipientEmail, subject, bodyHtml, bodyText);

        // Return success response
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = "OTP sent successfully", otp })
        };
    }
}
