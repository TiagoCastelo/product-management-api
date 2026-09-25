using System.Net;
using System.Net.Http.Json;

using ProductManagement.AcceptanceTests.Support;
using ProductManagement.Application.Products;
using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class FindingProductsSteps(ProductScenarioContext context)
{
    [When(@"I search the catalogue for ""([^""]+)""")]
    public async Task WhenISearchTheCatalogueFor(string term)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        context.LastResponse = await context.Client.GetAsync(ProductRoutes.Search(term), cancellationToken);
        context.LastResults = (await context.LastResponse.Content.ReadFromJsonAsync<List<ProductDto>>(cancellationToken))!;
    }

    [When(@"I list products with stock between (\d+) and (\d+)")]
    public async Task WhenIListProductsWithStockBetween(int min, int max)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        context.LastResponse = await context.Client.GetAsync(ProductRoutes.StockLevel($"min={min}&max={max}"), cancellationToken);
        context.LastResults = context.LastResponse.StatusCode == HttpStatusCode.OK
            ? (await context.LastResponse.Content.ReadFromJsonAsync<List<ProductDto>>(cancellationToken))!
            : [];
    }

    [Then(@"the results include ""([^""]+)""")]
    public void ThenTheResultsInclude(string name)
    {
        var id = context.Get(name).Id;

        Assert.Contains(context.LastResults, product => product.Id == id);
    }

    [Then(@"the results do not include ""([^""]+)""")]
    public void ThenTheResultsDoNotInclude(string name)
    {
        var id = context.Get(name).Id;

        Assert.DoesNotContain(context.LastResults, product => product.Id == id);
    }

    [Then(@"the request is rejected because the range is invalid")]
    public async Task ThenTheRequestIsRejectedBecauseTheRangeIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var problem = await ProblemResponses.ReadValidationProblemAsync(context.LastResponse, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, context.LastResponse.StatusCode);
        Assert.Contains("min", problem.Errors.Keys);
    }
}
