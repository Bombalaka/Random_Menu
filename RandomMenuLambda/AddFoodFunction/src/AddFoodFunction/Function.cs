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

namespace AddFoodFunction;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get foods table name from config helper
        var foodsTableName = await ConfigHelper.GetFoodsTableNameAsync();


        //we start with add food (post in api gateway)
        var foodItem = JsonSerializer.Deserialize<FoodItem>(request.Body);

        // validation check if deviceId and FoodName are not null or empty and foodItem is not null
        if (foodItem == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "foodItem is required" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        else if (string.IsNullOrEmpty(foodItem.deviceId))
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
        else if (string.IsNullOrEmpty(foodItem.FoodName))
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


        // Trim whitespace
        if (!string.IsNullOrWhiteSpace(foodItem.deviceId))
        {
            foodItem.deviceId = foodItem.deviceId.Trim().ToLower();
        }
        if (!string.IsNullOrWhiteSpace(foodItem.FoodName))
        {
            foodItem.FoodName = foodItem.FoodName.Trim().ToLower();
        }

        //Validation lenghts of deviceId, FoodName 
        if (foodItem.deviceId.Length < 10)
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
        if (foodItem.deviceId.Length > 50)
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

        
        //validation length of foodname
        if (foodItem.FoodName.Length < 2)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "FoodName must be at least 2 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        if (foodItem.FoodName.Length > 100)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new{error = "FoodName must be at most 100 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        //add ownership to food item to check if device is registered
        if(!await DeviceHelper.IsDeviceRegisteredAsync(foodItem.deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }

        //generate foodId
        foodItem.foodId = Guid.NewGuid().ToString();
        //connect dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();



        //save to dynamodb table Foods
        var putItemRequest = new PutItemRequest
            {
                TableName = foodsTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "deviceId", new AttributeValue(foodItem.deviceId) },
                    { "foodId", new AttributeValue(foodItem.foodId) },
                    { "FoodName", new AttributeValue(foodItem.FoodName) }
                }
            };
        await dynamoDbClient.PutItemAsync(putItemRequest);

        //return response with foodItem
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(foodItem),
            Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        }
        };
    }
}


