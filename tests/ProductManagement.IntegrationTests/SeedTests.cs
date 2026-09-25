using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ProductManagement.Application.Products;
using ProductManagement.Infrastructure.Persistence;
using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

public sealed class SeedTests(SqlServerAssemblyFixture fixture) : IAsyncLifetime
{
    private ProductApiFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        var connectionString = await fixture.SqlServer.CreateDatabaseAsync($"seed{Guid.NewGuid():N}", TestContext.Current.CancellationToken);
        _factory = new ProductApiFactory(connectionString);
    }

    public ValueTask DisposeAsync()
    {
        _factory.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task ListAll_AfterMigration_MeetsSeedRules()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.CreateClient();

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products", cancellationToken);

        Assert.NotNull(products);
        Assert.True(products!.Count >= 20);
        Assert.Equal(products.Count, products.Select(p => p.Sku).Distinct().Count());
        Assert.True(products.Count(p => p.Stock == 0) >= 2);
        Assert.True(products.Count(p => p.Stock is >= 1 and <= 5) >= 3);
        Assert.True(products.Count(p => p.Stock >= 100) >= 5);
        Assert.True(products.Min(p => p.Id) >= 100000);
    }

    [Fact]
    public async Task MigrateAsync_CalledAgainAfterSeeding_AddsNoRows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var initialCount = await context.Products.CountAsync(cancellationToken);

        await context.Database.MigrateAsync(cancellationToken);
        var finalCount = await context.Products.CountAsync(cancellationToken);

        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task ListAll_SecondDatabaseSeededInSameProcess_SeedIdsStartAtMinimumAndNewCreateGetsUnusedId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var secondConnectionString = await fixture.SqlServer.CreateDatabaseAsync($"seedsecond{Guid.NewGuid():N}", cancellationToken);
        using var secondFactory = new ProductApiFactory(secondConnectionString);
        var client = secondFactory.CreateClient();

        var seeded = await client.GetFromJsonAsync<List<ProductDto>>("/api/products", cancellationToken);
        var seededIds = seeded!.Select(p => p.Id).ToHashSet();
        var createResponse = await client.PostAsJsonAsync("/api/products", ProductPayloads.ValidCreate(), cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

        Assert.Equal(100000, seeded!.Min(p => p.Id));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.DoesNotContain(created!.Id, seededIds);
    }
}
