using System.Collections.Generic;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RandomMenuLambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Request received!");

        string name = "world"; // default name if did add any quesry on so it will add this name in the response
       // Check if query parameters exist
            if (request.QueryStringParameters != null)
            {
                context.Logger.LogLine("Query parameters found!");
                
                // Check if 'name' parameter exists
                if (request.QueryStringParameters.ContainsKey("name"))
                {
                    name = request.QueryStringParameters["name"];
                    context.Logger.LogLine($"Name parameter: {name}");
                }
                else
                {
                    context.Logger.LogLine("No 'name' parameter found");
                }
            }
            else
            {
                context.Logger.LogLine("No query parameters at all");
            }
            string message = $"Hello, {name}!";
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = message,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }
    
}
