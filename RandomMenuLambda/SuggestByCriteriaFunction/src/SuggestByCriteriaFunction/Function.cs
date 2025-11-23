using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Text.Json;
using RandomMenuLambda.Models;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SuggestByCriteriaFunction;

public class Function
{
    private static readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {

        //get deviceId from query parameters
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey("criteria"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "Criteria is required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        var criteria = request.QueryStringParameters["criteria"];

        //trim whitespace
        criteria = criteria.Trim().ToLower();

        //build ai promt : system promt
        var systemPrompt = $"You are a helpful food recommendation assistant that suggests foods based on specific criteria. Always provide practical, delicious suggestions with complete recipes in valid JSON format.";

        //build ai promt : user prompt
        var userPrompt = $@"Suggest ONE food that matches this criteria: {criteria}

        Return ONLY valid JSON (no markdown) with this structure:
        {{
        ""suggestedFood"": ""name of suggested food"",
        ""reason"": ""brief explanation why it matches the criteria"",
        ""recipe"": {{
            ""title"": ""recipe name"",
            ""description"": ""brief description"",
            ""ingredients"": [""item with measurement""],
            ""instructions"": [""detailed step""],
            ""prepTime"": ""X minutes"",
            ""cookTime"": ""X minutes"",
            ""servings"": number,
            ""difficulty"": ""Easy/Medium/Hard""
        }}
        }}";
                    

        //build bedrock request(call to bedrock)
        var bedrockRequest = new InvokeModelRequest
        {
            ModelId = "anthropic.claude-3-haiku-20240307-v1:0",
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new 
            { 
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = 1000,
                temperature = 1.0,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            })))

        };

        //invoke bedrock(api call to bedrock)
        var bedrockResponse = await _bedrockRuntimeClient.InvokeModelAsync(bedrockRequest);

        //read response stream
        using var reader = new StreamReader(bedrockResponse.Body);
        //convert to string
        var responseBody = await reader.ReadToEndAsync();
        
        //Parse Bedrock response wrapper JSON
        var bedrockResponseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
        //extract content array
        var content = bedrockResponseJson.GetProperty("content");
        //get first content item
        var text = content[0].GetProperty("text").GetString();

        // check what AI returned:
        context.Logger.LogLine($"AI Response Text: {text}");

        
        try{
            //parse suggested food name and reason json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var criteriaSuggestion = JsonSerializer.Deserialize<CriteriaSuggestion>(text, options);

            //build criteria suggestion response(how ai returned the data)
            var SuggestionResponse = new CriteriaSuggestion
            {
               Criteria = criteria, 
               SuggestedFood = criteriaSuggestion.SuggestedFood,
               Reason = criteriaSuggestion.Reason,
               Recipe = criteriaSuggestion.Recipe
               
            };
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(SuggestionResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        //Deserialize JSON text to CriteriaSuggestion object
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error parsing criteria suggestion: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to parse criteria suggestion" }),
            };
        }

    }
}
