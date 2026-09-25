using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ProductManagement.Application.Products;
using ProductManagement.Infrastructure.Persistence;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class IdGenerationTests(SqlServerAssemblyFixture fixture) : IAsyncLifetime
{
    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync() =>
        _connectionString = await fixture.SqlServer.CreateDatabaseAsync($"idgen{Guid.NewGuid():N}", TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Create_FiftyConcurrentRequestsAcrossTwoInstances_AllSucceedWithDistinctIdsInRange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var instanceA = new ProductApiFactory(_connectionString);
        using var instanceB = new ProductApiFactory(_connectionString);
        var clientA = instanceA.CreateClient();
        var clientB = instanceB.CreateClient();

        var creationsA = Enumerable.Range(0, 25).Select(_ => CreateProductAsync(clientA, cancellationToken));
        var creationsB = Enumerable.Range(0, 25).Select(_ => CreateProductAsync(clientB, cancellationToken));
        var responses = await Task.WhenAll(creationsA.Concat(creationsB));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

        var ids = await Task.WhenAll(responses.Select(response => ReadIdAsync(response, cancellationToken)));

        Assert.Equal(50, ids.Length);
        Assert.Equal(50, ids.Distinct().Count());
        Assert.All(ids, id => Assert.InRange(id, 100000, 999999));
    }

    [Fact]
    public async Task Create_SequenceRestartedAtMaximum_LastIdSucceedsAndNextReturnsIdSpaceExhausted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var instance = new ProductApiFactory(_connectionString);
        using var scope = instance.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await context.Database.ExecuteSqlRawAsync(
            $"ALTER SEQUENCE {ProductConfiguration.IdSequenceName} RESTART WITH {ProductConfiguration.IdSequenceMax}", cancellationToken);
        var client = instance.CreateClient();

        var first = await CreateProductAsync(client, cancellationToken);
        var firstProduct = await first.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        var second = await CreateProductAsync(client, cancellationToken);
        var problem = await ProblemResponses.ReadProblemAsync(second, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(ProductConfiguration.IdSequenceMax, firstProduct!.Id);
        Assert.Equal(HttpStatusCode.InternalServerError, second.StatusCode);
        Assert.Equal("Product ID space exhausted", problem.Title);
    }

    private static Task<HttpResponseMessage> CreateProductAsync(HttpClient client, CancellationToken cancellationToken) =>
        client.PostAsJsonAsync(ProductRoutes.Products, ProductPayloads.ValidCreate(), cancellationToken);

    private static async Task<int> ReadIdAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        (await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken))!.Id;
}
