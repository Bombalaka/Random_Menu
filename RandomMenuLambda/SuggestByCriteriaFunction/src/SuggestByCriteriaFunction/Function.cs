using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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

namespace SuggestByCriteriaFunction;

public class Function
{
    private static readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient = new AmazonBedrockRuntimeClient();
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {

        //get suggestion history table name from config helper
        var suggestionHistoryTableName = await ConfigHelper.GetSuggestionHistoryTableNameAsync();
        //connect dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();

        //get criteria from query parameters
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey("criteria") || 
            !request.QueryStringParameters.ContainsKey("deviceId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "Criteria is required and deviceId is required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        var criteria = request.QueryStringParameters["criteria"];
        var deviceId = request.QueryStringParameters["deviceId"];
        
        //trim whitespace
        criteria = criteria.Trim().ToLower();
        deviceId = deviceId.Trim().ToLower();
        
        //validation check if deviceID is existing
        if(!await DeviceHelper.IsDeviceRegisteredAsync(deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }


        //calculate date 7 days ago
        DateTime sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        //query past CRITERIA suggestions (not favorites!)
        var criteriaSuggestionQueryRequest = new QueryRequest
        {
            TableName = suggestionHistoryTableName,
            KeyConditionExpression = "deviceId = :v_deviceId AND suggestionDate >= :v_date",
            FilterExpression = "suggestionType = :v_suggestionType",//filter only criteria suggestions
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_deviceId", new AttributeValue {S = deviceId}},
                { ":v_date", new AttributeValue {S = sevenDaysAgo.ToString("o")}},
                { ":v_suggestionType", new AttributeValue {S = "criteria" }}
            }
        };
        //execute query
        var criteriaSuggestionResponse = await dynamoDbClient.QueryAsync(criteriaSuggestionQueryRequest);
        //Extract previousSuggestions list
        var previousCriteriaSuggestions  = criteriaSuggestionResponse.Items
            .Select(item => item["suggestedFood"].S)
            .ToList();

        //build exclusion text for AI
        var exclusionText = "";
        if (previousCriteriaSuggestions.Count > 0)
        {
            exclusionText = $"\n\nIMPORTANT: You have ALREADY suggested these foods: {string.Join(", ", previousCriteriaSuggestions)}\nDo NOT suggest any of these again. Suggest something COMPLETELY DIFFERENT and CREATIVE!.";
        }

        //build ai promt : system promt
        var systemPrompt = $"You are a food recommendation assistant. Your PRIMARY GOAL is to STRICTLY follow the user's specified criteria. You must NEVER suggest food that doesn't match the criteria. Always provide complete recipes in valid JSON format.";
        //build ai promt : user prompt
        var userPrompt = $@"USER'S CRITERIA: {criteria}
        {exclusionText}

        CRITICAL INSTRUCTIONS:
        1. If criteria mentions SPECIFIC CUISINE (e.g., ""Korean"", ""Japanese""), you MUST suggest ONLY from that cuisine
        2. If criteria is GENERAL (e.g., ""spicy"", ""quick"", ""healthy""), explore DIFFERENT world cuisines
        3. NEVER suggest food from different cuisine than specified

        VARIETY AND CREATIVITY RULES:
        - Avoid obvious/common suggestions (like always suggesting Curry for spicy)
        - Rotate between different cuisines: Korean, Thai, Mexican, Indian, Japanese, Chinese, Vietnamese, Middle Eastern, etc.
        - Consider lesser-known regional dishes
        - Think creatively!

        EXAMPLES:
        General criteria ""spicy"":
        - Day 1: Thai Tom Yum Soup
        - Day 2: Korean Tteokbokki  
        - Day 3: Mexican Salsa Verde Chicken
        - Day 4: Indian Vindaloo
        - Day 5: Sichuan Mapo Tofu

        Specific cuisine ""Korean spicy"":
        - MUST be Korean: Kimchi Jjigae, NOT Curry

        YOUR TASK:
        Suggest ONE food that matches criteria: {criteria}

        If cuisine not specified, pick a RANDOM/CREATIVE option from world cuisines!
        If cuisine IS specified, STRICTLY follow it!

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

            //save type and criteria to suggestion history
            var currentDate = DateTime.UtcNow.ToString("o");
            var putItemRequest = new PutItemRequest
            {
                TableName = suggestionHistoryTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "deviceId", new AttributeValue { S = deviceId } },
                    { "suggestionDate", new AttributeValue { S = currentDate } },
                    { "suggestionType", new AttributeValue { S = "criteria" } },
                    { "criteriaUsed", new AttributeValue { S = criteria } },
                    { "suggestedFood", new AttributeValue { S = criteriaSuggestion.SuggestedFood } },
                    { "Reason", new AttributeValue { S = criteriaSuggestion.Reason } },
                    
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
