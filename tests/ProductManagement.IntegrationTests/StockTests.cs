using System.Net;
using System.Net.Http.Json;

using ProductManagement.Application.Products;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class StockTests(SqlServerAssemblyFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task DecrementStock_SufficientStock_Returns200WithUpdatedStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 10, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.DecrementStock(created.Id, "4"), null, cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(6, product!.Stock);
    }

    [Fact]
    public async Task DecrementStock_ExceedsStock_Returns409AndLeavesStockUnchanged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 3, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.DecrementStock(created.Id, "10"), null, cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(response, cancellationToken);
        var current = await _client.GetFromJsonAsync<ProductDto>(ProductRoutes.Product(created.Id), cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Insufficient stock", problem.Title);
        Assert.Equal(3, current!.Stock);
    }

    [Fact]
    public async Task DecrementStock_UnknownProduct_Returns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.PostAsync(ProductRoutes.DecrementStock(999995, "1"), null, cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddStock_ValidQuantity_Returns200WithUpdatedStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 10, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.AddStock(created.Id, "5"), null, cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(15, product!.Stock);
    }

    [Fact]
    public async Task AddStock_UnknownProduct_Returns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.PostAsync(ProductRoutes.AddStock(999994, "1"), null, cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddStock_BeyondIntMaxValue_Returns409AndLeavesStockUnchanged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = int.MaxValue - 5, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.AddStock(created.Id, "10"), null, cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(response, cancellationToken);
        var current = await _client.GetFromJsonAsync<ProductDto>(ProductRoutes.Product(created.Id), cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Stock overflow", problem.Title);
        Assert.Equal(int.MaxValue - 5, current!.Stock);
    }

    [Fact]
    public async Task DecrementStock_ZeroQuantity_Returns400WithQuantityError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.DecrementStock(created.Id, "0"), null, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("quantity", problem.Errors.Keys);
    }

    [Fact]
    public async Task AddStock_ZeroQuantity_Returns400WithQuantityError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.AddStock(created.Id, "0"), null, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("quantity", problem.Errors.Keys);
    }

    [Fact]
    public async Task DecrementStock_NonNumericQuantity_Returns400()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.DecrementStock(created.Id, "abc"), null, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddStock_NonNumericQuantity_Returns400()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.PostAsync(ProductRoutes.AddStock(created.Id, "abc"), null, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
