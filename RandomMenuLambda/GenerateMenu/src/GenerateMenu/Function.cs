using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using RandomMenuLambda.Models;
using System.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GenerateMenu;

public class Function
{


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        //get foods table name from config helper
        var foodsTableName = await ConfigHelper.GetFoodsTableNameAsync();
        
        
        //Get deviceId from query parameters
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

        //Validate deviceId
        var deviceId = request.QueryStringParameters["deviceId"];

        //trim whitespace
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "deviceId is required and cannot be empty" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        deviceId = deviceId.Trim().ToLower();

        //validation length of deviceId
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
         //Check if device is registered
        if(!await DeviceHelper.IsDeviceRegisteredAsync(deviceId))
        {
            return DeviceHelper.CreateUnregisteredDeviceResponse();
        }

        // create dynamo db client
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
        var foodListResponse = await dynamoDbClient.QueryAsync(queryRequest);

        //convert to List<FoodItem>
        var foodList = foodListResponse.Items
            .Select(item => new FoodItem
            {
                deviceId = item["deviceId"].S,
                foodId = item["foodId"].S,
                FoodName = item["FoodName"].S
            })
            .ToList();
        //check if foods list is empty
        if (foodList.Count == 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = JsonSerializer.Serialize(new { error = "No foods found" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }


        //get random food 
        var random = new Random();
        var randomIndex = random.Next(0, foodList.Count);
        var randomFood = foodList[randomIndex];


        //return response
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new {
                menu = randomFood,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }), 
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
                
            }
        }; 
    }
}
