using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using RandomMenuLambda.Models;
using System.Text.RegularExpressions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteFoodFunction;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get foods table name from config helper
        var foodsTableName = await ConfigHelper.GetFoodsTableNameAsync();

        //Get deviceId and foodId from query request withour from body
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey("deviceId") || !request.QueryStringParameters.ContainsKey("foodId"))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId and foodId are required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        // validate deviceId and foodId is exist
        var deviceId = request.QueryStringParameters["deviceId"];
        var foodId = request.QueryStringParameters["foodId"];

        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(foodId))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId and foodId are required and cannot be empty" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        //trim whitespace
        deviceId = deviceId.Trim().ToLower();
        foodId = foodId.Trim().ToLower();

        //validation length of deviceId and foodId
        if (deviceId.Length < 10)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId must be at least 10 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        if (deviceId.Length > 50)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId must be at most 50 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        if (foodId.Length < 10)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "foodId must be at least 10 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        if (foodId.Length > 50)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "foodId must be at most 50 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        //Check if device is registered
        if(!await DeviceHelper.IsDeviceRegisteredAsync(deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }

        //connect dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();

        // Create DynamoDB DeleteItem request
        var deleteRequest = new DeleteItemRequest
        {
            TableName = foodsTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "deviceId", new AttributeValue(deviceId) },
                { "foodId", new AttributeValue(foodId) }
            }
        };
    
        try
        {
            //execute delete
            var deleteResponse = await dynamoDbClient.DeleteItemAsync(deleteRequest);
            //return response message 
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { message = "Food deleted successfully" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error deleting food: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to delete food" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }
}

