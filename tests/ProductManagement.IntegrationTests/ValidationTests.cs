using System.Net;
using System.Net.Http.Json;

using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class ValidationTests(SqlServerAssemblyFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();

    [Theory]
    [InlineData("sku")]
    [InlineData("name")]
    [InlineData("price")]
    [InlineData("stock")]
    public async Task Create_MissingRequiredField_Returns400WithFieldError(string field)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload.Remove(field);

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemResponses.MediaType, response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(field, problem.Errors.Keys);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("sku-lower-case")]
    [InlineData("SKU_UNDERSCORE")]
    public async Task Create_InvalidSkuPattern_Returns400WithSkuError(string invalidSku)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate(invalidSku);

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("sku", problem.Errors.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyOrWhitespaceName_Returns400WithNameError(string invalidName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["name"] = invalidName;

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_NameTooLong_Returns400WithNameError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["name"] = new string('A', 101);

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_DescriptionTooLong_Returns400WithDescriptionError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["description"] = new string('A', 501);

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("description", problem.Errors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000000.01)]
    public async Task Create_PriceOutOfRange_Returns400WithPriceError(decimal invalidPrice)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["price"] = invalidPrice;

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("price", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_PriceWithTooManyDecimals_Returns400WithPriceError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["price"] = 49.999m;

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("price", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_PriceWithTrailingZeroDecimal_Returns201()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["price"] = 49.990m;

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_NegativeStock_Returns400WithStockError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = ProductPayloads.ValidCreate();
        payload["stock"] = -1;

        var response = await _client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("stock", problem.Errors.Keys);
    }

    [Fact]
    public async Task Update_MissingRowVersion_Returns400WithRowVersionError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);
        var payload = ProductPayloads.ValidUpdate(created.RowVersion);
        payload.Remove("rowVersion");

        var response = await _client.PutAsJsonAsync(ProductRoutes.Product(created.Id), payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("rowVersion", problem.Errors.Keys);
    }

    [Fact]
    public async Task Update_PriceWithTooManyDecimals_Returns400WithPriceError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);
        var payload = ProductPayloads.ValidUpdate(created.RowVersion);
        payload["price"] = 49.999m;

        var response = await _client.PutAsJsonAsync(ProductRoutes.Product(created.Id), payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("price", problem.Errors.Keys);
    }

    [Fact]
    public async Task Update_WhitespaceName_Returns400WithNameError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var created = await ProductTestClient.CreateAsync(_client, null, cancellationToken);
        var payload = ProductPayloads.ValidUpdate(created.RowVersion);
        payload["name"] = "   ";

        var response = await _client.PutAsJsonAsync(ProductRoutes.Product(created.Id), payload, cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Search_MissingName_Returns400WithNameError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(new Uri("/api/products/search", UriKind.Relative), cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_EmptyOrWhitespaceName_Returns400WithNameError(string blankName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.Search(blankName), cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task Search_NameTooLong_Returns400WithNameError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.Search(new string('A', 101)), cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("name", problem.Errors.Keys);
    }

    [Fact]
    public async Task StockLevel_MissingMin_Returns400()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.StockLevel("max=10"), cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemResponses.MediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StockLevel_MissingMax_Returns400()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.StockLevel("min=0"), cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemResponses.MediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StockLevel_NegativeMin_Returns400WithMinError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.StockLevel("min=-1&max=10"), cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("min", problem.Errors.Keys);
    }

    [Fact]
    public async Task StockLevel_MinGreaterThanMax_Returns400WithMinError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(ProductRoutes.StockLevel("min=10&max=5"), cancellationToken);
        var problem = await ProblemResponses.ReadValidationProblemAsync(response, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("min", problem.Errors.Keys);
    }
}
