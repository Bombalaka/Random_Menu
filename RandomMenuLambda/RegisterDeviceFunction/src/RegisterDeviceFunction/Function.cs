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

namespace RegisterDeviceFunction;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get table name from config helper
        var deviceRegistryTableName = await ConfigHelper.GetDeviceRegistryTableNameAsync();

        //deserialize request body
        var deviceRegistration = JsonSerializer.Deserialize<DeviceRegistration>(request.Body);

        //validation check deviceid and username  is existing
        if (string.IsNullOrEmpty(deviceRegistration.deviceId) || string.IsNullOrEmpty(deviceRegistration.username))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId and username are required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        //trim whitespace
        deviceRegistration.deviceId = deviceRegistration.deviceId.Trim().ToLower();
        deviceRegistration.username = deviceRegistration.username.Trim().ToLower();


        //validation length of deviceId and username
        if (deviceRegistration.deviceId.Length < 10 || deviceRegistration.deviceId.Length > 50)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId must be between 10 and 50 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        if (deviceRegistration.username.Length < 2 || deviceRegistration.username.Length > 50)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "username must be between 2 and 50 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
       
        //check if device is already registered and get device information
        //var isRegistered = await DeviceHelper.IsDeviceRegisteredAsync(deviceRegistration.deviceId);
        var deviceInfo = await DeviceHelper.GetDeviceInfoAsync(deviceRegistration.deviceId);
        if (deviceInfo != null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { 
                    message = "Device is already registered",
                    deviceId = deviceInfo.deviceId,
                    username = deviceInfo.username }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        //create dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();

        //putitem to dynamodb table with deviceRegistration
        var putItemRequest = new PutItemRequest
        {
            TableName = deviceRegistryTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "deviceId", new AttributeValue(deviceRegistration.deviceId) },
                { "username", new AttributeValue(deviceRegistration.username) },
                { "createdAt", new AttributeValue(DateTime.UtcNow.ToString("o")) },
                { "lastLogin", new AttributeValue(DateTime.UtcNow.ToString("o")) }
            }
        };
        
        //execute with error handling 
        try
        {
            await dynamoDbClient.PutItemAsync(putItemRequest);
            //return response
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { message = "Device registered successfully",
                deviceId = deviceRegistration.deviceId,
                username = deviceRegistration.username }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error registering device: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to register device" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }
}
