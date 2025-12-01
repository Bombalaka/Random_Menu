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
using System.Linq;



// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SuggestNewFoodFunction;

public class Function
{
    private static readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();
   
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get suggestion history table name from config helper
        var suggestionHistoryTableName = await ConfigHelper.GetSuggestionHistoryTableNameAsync();

        //get deviceId from query parameters
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey("deviceId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId is required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        var deviceId = request.QueryStringParameters["deviceId"];

        //trim whitespace
        deviceId = deviceId.Trim().ToLower();

        //validation check if deviceID is existing
        if(!await DeviceHelper.IsDeviceRegisteredAsync(deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }

        //get foods table name from config helper
        var foodsTableName = await ConfigHelper.GetFoodsTableNameAsync();

        //connect dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();

        //query Dynamodb for all foods for the deviceId
        var queryRequest = new QueryRequest
        {
            TableName = foodsTableName,
            KeyConditionExpression = "deviceId = :v_deviceId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_deviceId", new AttributeValue {S = deviceId}}  
            }
        };
        
        //execute query
        var response = await dynamoDbClient.QueryAsync(queryRequest);

        //get list of user's favorite names
        var favoriteFoodNames = response.Items
            .Select(item => item["FoodName"].S)
            .ToList();

        //calculate date 7 days ago
        DateTime sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        //query past suggestion
        var suggestionHistoryQueryRequest = new QueryRequest
        {
            TableName = suggestionHistoryTableName,
            KeyConditionExpression = "deviceId = :v_deviceId AND suggestionDate >= :v_date",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_deviceId", new AttributeValue {S = deviceId}},
                { ":v_date", new AttributeValue {S = sevenDaysAgo.ToString("o")}}
            }
        };
        //execute query
        var suggestionHistoryResponse = await dynamoDbClient.QueryAsync(suggestionHistoryQueryRequest);
        //Extract previousSuggestions list
        var previousSuggestions  = suggestionHistoryResponse.Items
            .Select(item => item["suggestedFood"].S)
            .ToList();

       

        
        //handle is user has no food saved
        if (favoriteFoodNames.Count == 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "No foods saved. Please add some favorite foods first! "}),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        //build exclusion text for AI
        var exclusionText = "";
        if (previousSuggestions.Count > 0)
        {
            exclusionText = $"\n\nIMPORTANT: You have ALREADY suggested these foods: {string.Join(", ", previousSuggestions)}\nDo NOT suggest any of these again. Suggest something COMPLETELY DIFFERENT.";
        }
        
        //build ai promt : system promt
        var systemPrompt = $"You are a food discovery assistant that helps users find new foods similar to their favorites. Analyze the user's taste preferences and suggest new foods they would likely enjoy. Always include a complete recipe in valid JSON format.";

        //build ai promt : user prompt
        var userPrompt = $@"Based on these favorite foods: {string.Join(", ", favoriteFoodNames)}
        {exclusionText}
        Suggest ONE new food that is similar but different from these favorites.
        Return ONLY valid JSON (no markdown) with this structure:
        {{
        ""suggestedFood"": ""name of new food"",
        ""reason"": ""brief explanation why user would like it"",
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

        // check what AI returned:
        context.Logger.LogLine($"AI Response Text: {text}");

        
        try{
            //parse suggested food name and reason json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var foodDiscovery = JsonSerializer.Deserialize<FoodDiscovery>(text, options);

            

            //build food discovery response(how ai returned the data)
            var foodDiscoveryResponse = new FoodDiscovery
            {
                SuggestedFood = foodDiscovery.SuggestedFood, //new food name suggested by AI
                Reason = foodDiscovery.Reason, //reason for the suggested food name
                Recipe = foodDiscovery.Recipe, //recipe object
                BasedOnFavorites = favoriteFoodNames //list of user's favorite foods (from dynamo db) which foods AI considered
            };

            //save new suggested food to suggestion history
            var currentDate = DateTime.UtcNow.ToString("o");
            var putItemRequest = new PutItemRequest
            {
                TableName = suggestionHistoryTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "deviceId", new AttributeValue { S = deviceId } },
                    { "suggestedFood", new AttributeValue { S = foodDiscovery.SuggestedFood } },
                    { "suggestionDate", new AttributeValue { S = currentDate } },
                    { "BasedOnFavorites", new AttributeValue { L = favoriteFoodNames.Select(food => new AttributeValue { S = food }).ToList() }},
                    { "Reason", new AttributeValue { S = foodDiscovery.Reason } },
                    { "suggestionType", new AttributeValue { S = "Favorite" } }
                    
                }
            };

            //execute put item request
            try{
                await dynamoDbClient.PutItemAsync(putItemRequest);
                context.Logger.LogLine("Suggestion saved to history successfully");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error saving suggestion history: {ex.Message}");
            }


            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(foodDiscoveryResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        //Deserialize JSON text to Recipe object
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error parsing suggested food name and reason: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to parse suggested food name and reason" }),
            };
        }

    }
}
