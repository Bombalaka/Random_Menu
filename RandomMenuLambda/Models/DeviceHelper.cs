using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace RandomMenuLambda.Models;

//shared class for device registration and management

public static class DeviceHelper
{
    public static async Task<bool> IsDeviceRegisteredAsync(string deviceId)
    {
        //get the register table name 
        var deviceRegistryTableName = await ConfigHelper.GetDeviceRegistryTableNameAsync();
        //create dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();
        //create get item request
        var getItemRequest = new GetItemRequest
        {
            TableName = deviceRegistryTableName,
            Key = new Dictionary<string, AttributeValue>
        {
            { "deviceId", new AttributeValue(deviceId) }
        }
        };
        //check the response
        try
        {
            //execute get item
            var getItemResponse = await dynamoDbClient.GetItemAsync(getItemRequest);
            //check if item exists
            return getItemResponse.Item != null && getItemResponse.Item.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting device registry: {ex.Message}");
            return false;
        }
    }

    //CHECK if device is registered 
    public static APIGatewayProxyResponse CreateUnregisteredDeviceResponse()
    {
       return new APIGatewayProxyResponse
       {
        StatusCode = 403,
        Body = JsonSerializer.Serialize(new { error = "Device is not registered" }),
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        }
       };
    }
    //Get devices information and return full device registration object  
    public static async Task<DeviceRegistration> GetDeviceInfoAsync(string deviceId)
    {
        try
        {
            //get the register table name 
            var deviceRegistryTableName = await ConfigHelper.GetDeviceRegistryTableNameAsync();
            //create dynamo db client
            var dynamoDbClient = new AmazonDynamoDBClient();
            //create get item request
            var getItemRequest = new GetItemRequest
            {
                TableName = deviceRegistryTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "deviceId", new AttributeValue(deviceId) }
                }
            };
            //execute get item and return DeviceRegistration
            var getItemResponse = await dynamoDbClient.GetItemAsync(getItemRequest);
            
            // Check if item exists
            if (getItemResponse.Item == null || getItemResponse.Item.Count == 0)
            {
                return null;
            }
            
            // Extract values from DynamoDB AttributeValue objects
            var item = getItemResponse.Item;
            return new DeviceRegistration
            {
                deviceId = item["deviceId"].S,
                username = item["username"].S,
                createdAt = DateTime.Parse(item["createdAt"].S),
                lastLogin = DateTime.Parse(item["lastLogin"].S)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting device information: {ex.Message}");
            return null;
        }
    }

}