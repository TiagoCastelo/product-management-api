using System.Net;
using System.Net.Http.Json;

using ProductManagement.AcceptanceTests.Support;
using ProductManagement.Application.Products;
using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class ProductCreationSteps(ProductScenarioContext context)
{
    private const decimal DefaultPrice = 9.99m;

    [Given(@"a product ""([^""]+)"" priced at (\d+(?:\.\d+)?) with (\d+) units in stock")]
    public async Task GivenAProductPricedAtWithUnitsInStock(string name, decimal price, int stock)
    {
        await CreateAsync(name, price, stock);
    }

    [Given(@"a product ""([^""]+)"" with (\d+) units in stock")]
    public async Task GivenAProductWithUnitsInStock(string name, int stock)
    {
        await CreateAsync(name, DefaultPrice, stock);
    }

    private async Task CreateAsync(string name, decimal price, int stock)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = new Dictionary<string, object?>
        {
            ["sku"] = TestIdentifiers.NewSku(),
            ["name"] = context.QualifiedName(name),
            ["description"] = null,
            ["price"] = price,
            ["stock"] = stock,
        };

        var response = await context.Client.PostAsJsonAsync(ProductRoutes.Products, payload, cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        context.Remember(name, product!);
    }
}
