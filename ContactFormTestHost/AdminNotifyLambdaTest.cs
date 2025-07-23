using System.Text.Json;
using Amazon.Lambda.SQSEvents;
using AdminNotifyLambda;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AdminNotifyLambdaTest
{
	public static async Task RunTest()
	{
		var function = new Function();

		await function.FunctionHandler(new SQSEvent
		{
			Records = new List<SQSEvent.SQSMessage>
			{
				new SQSEvent.SQSMessage
				{
					Body = JsonSerializer.Serialize(new {
						Name = "John Doe",
						Email = "john@example.com",
						Timestamp = System.DateTime.UtcNow.ToString()
					})
				}
			}
		}, null);

		Console.WriteLine("AdminNotifyLambda test executed");
	}
}
