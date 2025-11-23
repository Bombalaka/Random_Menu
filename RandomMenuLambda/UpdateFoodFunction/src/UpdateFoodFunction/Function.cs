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

namespace UpdateFoodFunction;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get table name from config helper
        var foodsTableName = await ConfigHelper.GetFoodsTableNameAsync();

        //deserialize request body to get deviceId foodId and new FoodName
        var foodItem = JsonSerializer.Deserialize<FoodItem>(request.Body);

        //validation check if deviceId, foodId and new FoodName are not null or empty and foodItem is not null
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
        else if (string.IsNullOrEmpty(foodItem.foodId))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "foodId is required" }),
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


        // Trim all fields

        if (!string.IsNullOrWhiteSpace(foodItem.deviceId))
        {
            foodItem.deviceId = foodItem.deviceId.Trim().ToLower();
        }
        if (!string.IsNullOrWhiteSpace(foodItem.FoodName))
        {
            foodItem.FoodName = foodItem.FoodName.Trim().ToLower();
        }
        if (!string.IsNullOrWhiteSpace(foodItem.foodId))
        {
            foodItem.foodId = foodItem.foodId.Trim().ToLower();
        }

        //Validation lenghts of deviceId
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
                Body = JsonSerializer.Serialize(new { error = "FoodName must be at most 100 characters" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        //validation length of foodId
        if (foodItem.foodId.Length < 10)
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
        if (foodItem.foodId.Length > 50)
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
        if(!await DeviceHelper.IsDeviceRegisteredAsync(foodItem.deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }

        //connect dynamo db client
        var dynamoDbClient = new AmazonDynamoDBClient();

        //create update request to dynamodb
        var updateRequest = new UpdateItemRequest
        {
            TableName = foodsTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "deviceId", new AttributeValue(foodItem.deviceId) },
                { "foodId", new AttributeValue(foodItem.foodId) }
            },
            UpdateExpression = "SET FoodName = :FoodName",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":FoodName", new AttributeValue(foodItem.FoodName) }
            },
            ReturnValues = "ALL_NEW"
        };
        


        //check if update was successful and return with update item
       try
        {
            //execute update request
            var updateResponse = await dynamoDbClient.UpdateItemAsync(updateRequest);
            // If we reach here, update succeeded!
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
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error updating food: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to update food" }),
                Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
            };
        }
    }
}
