using System.Globalization;
using System.Net;
using System.Net.Http.Json;

using ProductManagement.AcceptanceTests.Support;
using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class ValidationSteps(ProductScenarioContext context)
{
    private const string LongNamePlaceholder = "(101 characters)";

    [When(@"I try to add a product with (\w+) set to ""(.*)""")]
    public async Task WhenITryToAddAProductWithFieldSetTo(string field, string rawValue)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = new Dictionary<string, object?>
        {
            ["sku"] = TestIdentifiers.NewSku(),
            ["name"] = context.QualifiedName("Valid Product"),
            ["description"] = null,
            ["price"] = 9.99m,
            ["stock"] = 5,
            [field] = ResolveValue(field, rawValue),
        };

        context.LastResponse = await context.Client.PostAsJsonAsync(ProductRoutes.Products, payload, cancellationToken);
    }

    [Then(@"the request is rejected with a validation error for ""([^""]+)""")]
    public async Task ThenTheRequestIsRejectedWithAValidationErrorFor(string field)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var problem = await ProblemResponses.ReadValidationProblemAsync(context.LastResponse, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, context.LastResponse.StatusCode);
        Assert.Contains(field, problem.Errors.Keys);
    }

    private static object ResolveValue(string field, string rawValue)
    {
        if (rawValue == LongNamePlaceholder)
        {
            return new string('A', 101);
        }

        return field switch
        {
            "price" => decimal.Parse(rawValue, CultureInfo.InvariantCulture),
            "stock" => int.Parse(rawValue, CultureInfo.InvariantCulture),
            _ => rawValue,
        };
    }
}
