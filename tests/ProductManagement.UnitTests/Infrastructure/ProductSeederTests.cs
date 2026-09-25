using ProductManagement.Infrastructure.Persistence;

namespace ProductManagement.UnitTests.Infrastructure;

public class ProductSeederTests
{
    [Fact]
    public void CreateSeedProducts_Always_HasAtLeastTwentyProducts()
    {
        Assert.True(ProductSeeder.CreateSeedProducts().Count >= 20);
    }

    [Fact]
    public void CreateSeedProducts_Always_HasUniqueSkus()
    {
        var products = ProductSeeder.CreateSeedProducts();
        var uniqueSkuCount = products.Select(p => p.Sku).Distinct().Count();

        Assert.Equal(products.Count, uniqueSkuCount);
    }

    [Fact]
    public void CreateSeedProducts_Always_HasAtLeastTwoOutOfStockProducts()
    {
        Assert.True(ProductSeeder.CreateSeedProducts().Count(p => p.Stock == 0) >= 2);
    }

    [Fact]
    public void CreateSeedProducts_Always_HasAtLeastThreeLowStockProducts()
    {
        Assert.True(ProductSeeder.CreateSeedProducts().Count(p => p.Stock is >= 1 and <= 5) >= 3);
    }

    [Fact]
    public void CreateSeedProducts_Always_HasAtLeastFiveHighStockProducts()
    {
        Assert.True(ProductSeeder.CreateSeedProducts().Count(p => p.Stock >= 100) >= 5);
    }

    [Fact]
    public void CreateSeedProducts_Always_DoNotSetIdExplicitly()
    {
        Assert.All(ProductSeeder.CreateSeedProducts(), p => Assert.Equal(0, p.Id));
    }

    [Fact]
    public void CreateSeedProducts_CalledTwice_ReturnsDistinctInstancesWithIdZero()
    {
        var first = ProductSeeder.CreateSeedProducts();
        var second = ProductSeeder.CreateSeedProducts();

        Assert.All(first, p => Assert.Equal(0, p.Id));
        Assert.All(second, p => Assert.Equal(0, p.Id));
        Assert.All(first.Zip(second), pair => Assert.NotSame(pair.First, pair.Second));
    }
}
