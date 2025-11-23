using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using RandomMenuLambda.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetRecipeFunction;

public class Function
{
    private static readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get foodname from query parameter 
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey("FoodName"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "FoodName is required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        var foodName = request.QueryStringParameters["FoodName"];

        //trim whitespace
        foodName = foodName.Trim().ToLower();

        //create system promt (ccoking assistant)
        var systemPrompt = $"You are a helpful cooking assistant that generates practical, authentic recipes. Always format responses as valid JSON with the exact structure requested.";

        //create prompt (user prompt)
        var userPrompt = $@"Generate a detailed recipe for {foodName}.
        Return ONLY valid JSON (no markdown, no extra text) with this structure:
        {{
            ""title"": ""recipe name"",
            ""description"": ""brief description"",
            ""ingredients"": [""item with measurement""],
            ""instructions"": [""detailed step""],
            ""prepTime"": ""X minutes"",
            ""cookTime"": ""X minutes"",
            ""servings"": number,
            ""difficulty"": ""Easy/Medium/Hard""
        }}";

        //build bedrock request
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

        // ADD THIS LINE TO SEE WHAT AI RETURNED:
        context.Logger.LogLine($"AI Response Text: {text}");

        
        try{
            //parse recipe json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var recipe = JsonSerializer.Deserialize<Recipe>(text, options);

            //build recipe response
            var recipeResponse = new RecipeResponse
            {
                FoodName = foodName,
                Recipe = recipe,
                GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                GeneratedBy = "Bedrock Claude Haiku 3"
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(recipeResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };

        //Deserialize JSON text to Recipe object
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error parsing recipe: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to parse recipe" }),
            };
        }

    }
}