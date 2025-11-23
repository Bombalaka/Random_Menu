using System.Threading.Tasks;
using System;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;


namespace RandomMenuLambda.Models;

public static class ConfigHelper
{
    private static readonly AmazonSimpleSystemsManagementClient _ssmClient;
    //constructor
    static ConfigHelper()
    {
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }
    private static async Task<string> GetParameterAsync(string parameterName)
    {

        //create parameter request
        var parameterRequest = new GetParameterRequest
        {
            Name = parameterName
        };
        //call ssm client to get parameter
        var response = await _ssmClient.GetParameterAsync(parameterRequest);

        //extract and return value
        return response.Parameter.Value;
    }
    public static async Task<string> GetFoodsTableNameAsync()
    {
        try
        {
            return await GetParameterAsync("/random-menu-app/table-name");
        }
        catch (ParameterNotFoundException)
        {
            //Fallback to default value
            return "Foods";
        }
        catch (Exception ex)
        {
            //Log error and use default value
            Console.WriteLine($"Error getting foods table name: {ex.Message}");
            return "Foods";
        }
    }
    public static async Task<string> GetDeviceRegistryTableNameAsync()
    {
        var client = new AmazonSimpleSystemsManagementClient();
        var request = new GetParameterRequest
        {
            Name = "/random-menu-app/DeviceRegistryTableName"
        };
        var response = await client.GetParameterAsync(request);
        return response.Parameter.Value;
    }
    

}
