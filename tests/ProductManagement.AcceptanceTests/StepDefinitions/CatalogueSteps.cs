using System.Net;
using System.Net.Http.Json;

using ProductManagement.AcceptanceTests.Support;
using ProductManagement.Application.Products;
using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class CatalogueSteps(ProductScenarioContext context)
{
    [When(@"I add a new product ""([^""]+)"" priced at (\d+(?:\.\d+)?) with (\d+) units in stock")]
    public async Task WhenIAddANewProduct(string name, decimal price, int stock)
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

        context.LastResponse = await context.Client.PostAsJsonAsync(ProductRoutes.Products, payload, cancellationToken);
        var product = await context.LastResponse.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        context.Remember(name, product!);
    }

    [When(@"I view ""([^""]+)""")]
    public async Task WhenIView(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        context.LastResponse = await context.Client.GetAsync(ProductRoutes.Product(id), cancellationToken);
        var product = await context.LastResponse.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        context.Remember(name, product!);
    }

    [When(@"I rename ""([^""]+)"" to ""([^""]+)"" and change its price to (\d+(?:\.\d+)?)")]
    public async Task WhenIRenameAndChangePrice(string oldName, string newName, decimal price)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var existing = context.Get(oldName);
        var payload = new Dictionary<string, object?>
        {
            ["name"] = context.QualifiedName(newName),
            ["description"] = null,
            ["price"] = price,
            ["rowVersion"] = existing.RowVersion,
        };

        context.LastResponse = await context.Client.PutAsJsonAsync(ProductRoutes.Product(existing.Id), payload, cancellationToken);
        var updated = await context.LastResponse.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        context.Forget(oldName);
        context.Remember(newName, updated!);
    }

    [When(@"I remove ""([^""]+)"" from the catalogue")]
    public async Task WhenIRemove(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        context.LastResponse = await context.Client.DeleteAsync(ProductRoutes.Product(id), cancellationToken);
    }

    [When(@"I add another product with the same SKU as ""([^""]+)""")]
    public async Task WhenIAddAnotherProductWithTheSameSku(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = new Dictionary<string, object?>
        {
            ["sku"] = context.Get(name).Sku,
            ["name"] = context.QualifiedName($"Duplicate of {name}"),
            ["description"] = null,
            ["price"] = 9.99m,
            ["stock"] = 1,
        };

        context.LastResponse = await context.Client.PostAsJsonAsync(ProductRoutes.Products, payload, cancellationToken);
    }

    [Then(@"""([^""]+)"" is in the catalogue with a six-digit product id")]
    public void ThenIsInTheCatalogueWithASixDigitProductId(string name)
    {
        Assert.InRange(context.Get(name).Id, 100000, 999999);
    }

    [Then(@"""([^""]+)"" is priced at (\d+(?:\.\d+)?) with (\d+) units in stock")]
    public void ThenIsPricedAtWithUnitsInStock(string name, decimal price, int stock)
    {
        var product = context.Get(name);
        Assert.Equal(context.QualifiedName(name), product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(stock, product.Stock);
    }

    [Then(@"""([^""]+)"" can no longer be found")]
    public async Task ThenCanNoLongerBeFound(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        var response = await context.Client.GetAsync(ProductRoutes.Product(id), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Then(@"the request is rejected because the SKU is already in use")]
    public async Task ThenTheRequestIsRejectedBecauseTheSkuIsAlreadyInUse()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var problem = await ProblemResponses.ReadProblemAsync(context.LastResponse, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, context.LastResponse.StatusCode);
        Assert.Equal("Duplicate SKU", problem.Title);
    }
}
