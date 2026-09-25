using System.Net;
using System.Net.Http.Json;

using ProductManagement.Application.Products;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class ConcurrencyTests(SqlServerAssemblyFixture fixture)
{
    [Fact]
    public async Task DecrementStock_ManyConcurrentRequestsExceedingStock_ExactlyStockSucceedAndRestConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const int stock = 10;
        const int attempts = 25;
        using var secondInstance = new ProductApiFactory(fixture.SqlServer.ConnectionString);
        var clientA = fixture.Factory.CreateClient();
        var clientB = secondInstance.CreateClient();
        var created = await ProductTestClient.CreateAsync(clientA, payload => payload["stock"] = stock, cancellationToken);

        var attemptsFromA = Enumerable.Range(0, attempts / 2)
            .Select(_ => clientA.PostAsync(ProductRoutes.DecrementStock(created.Id, "1"), null, cancellationToken));
        var attemptsFromB = Enumerable.Range(0, attempts - (attempts / 2))
            .Select(_ => clientB.PostAsync(ProductRoutes.DecrementStock(created.Id, "1"), null, cancellationToken));
        var responses = await Task.WhenAll(attemptsFromA.Concat(attemptsFromB));

        var successCount = responses.Count(response => response.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(response => response.StatusCode == HttpStatusCode.Conflict);
        var final = await clientA.GetFromJsonAsync<ProductDto>(ProductRoutes.Product(created.Id), cancellationToken);

        Assert.Equal(stock, successCount);
        Assert.Equal(attempts - stock, conflictCount);
        Assert.Equal(0, final!.Stock);
    }
}
