using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

namespace RandomMenuLambda.Tests;

public class FunctionTest
{
    [Fact]
    public void TestToUpperFunction()
    {

        // Invoke the lambda function and confirm the string was upper cased.
        var function = new Function();
        var context = new TestLambdaContext();
        var upperCase = function.FunctionHandler(new APIGatewayProxyRequest(), context);

        Assert.Equal(200, upperCase.StatusCode);
    }
}
