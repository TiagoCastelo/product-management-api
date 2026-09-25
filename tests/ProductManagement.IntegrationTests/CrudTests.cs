using System.Net;
using System.Net.Http.Json;

using ProductManagement.Application.Products;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class CrudTests(SqlServerAssemblyFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task ListAll_Always_ReturnsSeededProducts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products", cancellationToken);

        Assert.NotNull(products);
        Assert.True(products!.Count >= 20);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocationAndBody()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sku = TestIdentifiers.NewSku();

        var response = await _client.PostAsJsonAsync("/api/products", ProductPayloads.ValidCreate(sku), cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/products/{product!.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal(sku, product.Sku);
        Assert.InRange(product.Id, 100000, 999999);
    }

    [Fact]
    public async Task GetById_ExistingProduct_Returns200WithProduct()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.GetAsync(ProductRoutes.Product(created.Id), cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Sku, product!.Sku);
    }

    [Fact]
    public async Task GetById_UnknownProduct_Returns404WithProductNotFoundTitle()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.Product(999998), cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemResponses.MediaType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Product not found", problem.Title);
    }

    [Fact]
    public async Task Update_ValidRequest_Returns200WithNewRowVersion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var response = await _client.PutAsJsonAsync(
            ProductRoutes.Product(created.Id), ProductPayloads.ValidUpdate(created.RowVersion), cancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Updated Product", updated!.Name);
        Assert.NotEqual(Convert.ToBase64String(created.RowVersion), Convert.ToBase64String(updated.RowVersion));
    }

    [Fact]
    public async Task Update_IgnoresSkuStockAndIdInBody_KeepsOriginalSkuAndStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);
        var payload = ProductPayloads.ValidUpdate(created.RowVersion);
        payload["sku"] = TestIdentifiers.NewSku();
        payload["stock"] = 999;
        payload["id"] = 1;

        var response = await _client.PutAsJsonAsync(ProductRoutes.Product(created.Id), payload, cancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Sku, updated!.Sku);
        Assert.Equal(created.Stock, updated.Stock);
        Assert.Equal(created.Id, updated.Id);
    }

    [Fact]
    public async Task Update_UnknownProduct_Returns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.PutAsJsonAsync(
            ProductRoutes.Product(999997), ProductPayloads.ValidUpdate([1, 2, 3]), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_StaleRowVersion_Returns409WithConcurrencyConflictTitleAndLeavesProductUnchanged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);
        var staleRowVersion = new byte[created.RowVersion.Length];

        var response = await _client.PutAsJsonAsync(
            ProductRoutes.Product(created.Id), ProductPayloads.ValidUpdate(staleRowVersion), cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(response, cancellationToken);
        var current = await _client.GetFromJsonAsync<ProductDto>(ProductRoutes.Product(created.Id), cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Concurrency conflict", problem.Title);
        Assert.Equal(created.Name, current!.Name);
        Assert.Equal(Convert.ToBase64String(created.RowVersion), Convert.ToBase64String(current.RowVersion));
    }

    [Fact]
    public async Task Create_DuplicateSku_Returns409AndDoesNotCreateSecondProduct()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sku = TestIdentifiers.NewSku();
        var name = $"Duplicate Sku Test {TestIdentifiers.NewToken()}";
        await ProductTestClient.CreateAsync(_client, payload => { payload["sku"] = sku; payload["name"] = name; }, cancellationToken);
        var duplicatePayload = ProductPayloads.ValidCreate(sku);
        duplicatePayload["name"] = name;

        var response = await _client.PostAsJsonAsync("/api/products", duplicatePayload, cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(response, cancellationToken);
        var matches = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search(name), cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Duplicate SKU", problem.Title);
        Assert.Single(matches!);
    }

    [Fact]
    public async Task Delete_ExistingProduct_Returns204AndSubsequentGetReturns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);

        var deleteResponse = await _client.DeleteAsync(ProductRoutes.Product(created.Id), cancellationToken);
        var getResponse = await _client.GetAsync(ProductRoutes.Product(created.Id), cancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownProduct_Returns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.DeleteAsync(ProductRoutes.Product(999996), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
