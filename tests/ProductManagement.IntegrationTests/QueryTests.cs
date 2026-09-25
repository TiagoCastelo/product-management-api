using System.Net.Http.Json;

using ProductManagement.Application.Products;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class QueryTests(SqlServerAssemblyFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task Search_MultipleMatches_OrdersByNameCaseInsensitiveThenId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var token = TestIdentifiers.NewToken();
        var b = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} b", cancellationToken);
        var a = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} A", cancellationToken);
        var c = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} C", cancellationToken);
        var dLow = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} D", cancellationToken);
        var dHigh = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} D", cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search(token), cancellationToken);

        Assert.Equal([a.Id, b.Id, c.Id, dLow.Id, dHigh.Id], results!.Select(p => p.Id));
    }

    [Fact]
    public async Task Search_PercentInTerm_MatchesLiterally()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var token = TestIdentifiers.NewToken();
        var exactMatch = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} 5%0 Widget", cancellationToken);
        await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} 500 Widget", cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search($"{token} 5%0"), cancellationToken);

        Assert.Equal([exactMatch.Id], results!.Select(p => p.Id));
    }

    [Fact]
    public async Task Search_UnderscoreInTerm_MatchesLiterally()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var token = TestIdentifiers.NewToken();
        var exactMatch = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} A_B Widget", cancellationToken);
        await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} AXB Widget", cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search($"{token} A_B"), cancellationToken);

        Assert.Equal([exactMatch.Id], results!.Select(p => p.Id));
    }

    [Fact]
    public async Task Search_OpenBracketInTerm_MatchesLiterally()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var token = TestIdentifiers.NewToken();
        var exactMatch = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} [A] Widget", cancellationToken);
        await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} A Widget", cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search($"{token} [A]"), cancellationToken);

        Assert.Equal([exactMatch.Id], results!.Select(p => p.Id));
    }

    [Fact]
    public async Task Search_DifferentCasing_MatchesCaseInsensitively()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var token = TestIdentifiers.NewToken();
        var created = await ProductTestClient.CreateAsync(_client, payload => payload["name"] = $"{token} Apple Pie", cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(ProductRoutes.Search($"{token.ToUpperInvariant()} apple"), cancellationToken);

        Assert.Equal([created.Id], results!.Select(p => p.Id));
    }

    [Fact]
    public async Task StockLevel_InclusiveRange_ReturnsMatchingProductsByIdAndExcludesOutliers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var below = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 41, cancellationToken);
        var atMin = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 42, cancellationToken);
        var atMax = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 58, cancellationToken);
        var above = await ProductTestClient.CreateAsync(_client, payload => payload["stock"] = 59, cancellationToken);

        var results = await _client.GetFromJsonAsync<List<ProductDto>>(
            ProductRoutes.StockLevel("min=42&max=58"), cancellationToken);
        var ids = results!.Select(p => p.Id).ToHashSet();

        Assert.Contains(atMin.Id, ids);
        Assert.Contains(atMax.Id, ids);
        Assert.DoesNotContain(below.Id, ids);
        Assert.DoesNotContain(above.Id, ids);
    }
}
