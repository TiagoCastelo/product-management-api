using System.Net;
using System.Net.Http.Json;

using ProductManagement.AcceptanceTests.Support;
using ProductManagement.Application.Products;
using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class StockMovementSteps(ProductScenarioContext context)
{
    [When(@"I receive (\d+) units of ""([^""]+)""")]
    public async Task WhenIReceiveUnitsOf(int quantity, string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        context.LastResponse = await context.Client.PostAsync(ProductRoutes.AddStock(id, $"{quantity}"), null, cancellationToken);
    }

    [When(@"I sell (\d+) units of ""([^""]+)""")]
    public async Task WhenISellUnitsOf(int quantity, string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        context.LastResponse = await context.Client.PostAsync(ProductRoutes.DecrementStock(id, $"{quantity}"), null, cancellationToken);
    }

    [Then(@"""([^""]+)"" has (\d+) units in stock")]
    public async Task ThenHasUnitsInStock(string name, int stock)
    {
        var product = await GetCurrentAsync(name);
        Assert.Equal(stock, product.Stock);
    }

    [Then(@"""([^""]+)"" still has (\d+) units in stock")]
    public async Task ThenStillHasUnitsInStock(string name, int stock)
    {
        var product = await GetCurrentAsync(name);
        Assert.Equal(stock, product.Stock);
    }

    [Then(@"the sale is rejected because there is not enough stock")]
    public async Task ThenTheSaleIsRejectedBecauseThereIsNotEnoughStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var problem = await ProblemResponses.ReadProblemAsync(context.LastResponse, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, context.LastResponse.StatusCode);
        Assert.Equal("Insufficient stock", problem.Title);
    }

    private async Task<ProductDto> GetCurrentAsync(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = context.Get(name).Id;

        var response = await context.Client.GetAsync(ProductRoutes.Product(id), cancellationToken);
        return (await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken))!;
    }
}
