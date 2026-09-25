using ProductManagement.Application.Products;
using ProductManagement.Domain.Products;

namespace ProductManagement.UnitTests.Application;

public class ProductServiceTests
{
    [Fact]
    public async Task ListAllAsync_Always_ReturnsMappedProducts()
    {
        var repository = Substitute.For<IProductRepository>();
        var product = Product.Create("SKU-001", "Widget", "A widget", 9.99m, 4);
        repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Product>)[product]);
        var sut = new ProductService(repository);

        var result = await sut.ListAllAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(product.Sku, result[0].Sku);
        Assert.Equal(product.Stock, result[0].Stock);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSuccessWithDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var product = Product.Create("SKU-002", "Widget", null, 9.99m, 4);
        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);
        var sut = new ProductService(repository);

        var result = await sut.GetByIdAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(product.Sku, result.Value.Sku);
    }

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNotFoundError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var sut = new ProductService(repository);

        var result = await sut.GetByIdAsync(99, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.NotFound, result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidCommand_ReturnsSuccessWithMappedDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("SKU-003", "Gadget", "A gadget", 15.50m, 3);
        var created = Product.Create(command.Sku, command.Name, command.Description, command.Price, command.Stock);
        repository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(created);
        var sut = new ProductService(repository);

        var result = await sut.CreateAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.Sku, result.Value.Sku);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSku_ReturnsDuplicateSkuError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("SKU-004", "Gadget", null, 15.50m, 3);
        repository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.DuplicateSku);
        var sut = new ProductService(repository);

        var result = await sut.CreateAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.DuplicateSku, result.Error);
    }

    [Fact]
    public async Task CreateAsync_IdSpaceExhausted_ReturnsIdSpaceExhaustedError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("SKU-005", "Gadget", null, 15.50m, 3);
        repository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.IdSpaceExhausted);
        var sut = new ProductService(repository);

        var result = await sut.CreateAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.IdSpaceExhausted, result.Error);
    }

    [Fact]
    public async Task UpdateDetailsAsync_MissingId_ReturnsNotFoundError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new UpdateProductDetailsCommand("New Name", null, 10.00m, []);
        repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var sut = new ProductService(repository);

        var result = await sut.UpdateDetailsAsync(99, command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateDetailsAsync_ValidCommand_ReturnsSuccessWithUpdatedDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var existing = Product.Create("SKU-006", "Old Name", "Old description", 10.00m, 5);
        var command = new UpdateProductDetailsCommand("New Name", "New description", 20.00m, [1, 2, 3]);
        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(existing);
        var sut = new ProductService(repository);

        var result = await sut.UpdateDetailsAsync(1, command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.Name);
    }

    [Fact]
    public async Task UpdateDetailsAsync_ValidCommand_PassesProductWithOriginalSkuAndStockToRepository()
    {
        var repository = Substitute.For<IProductRepository>();
        var existing = Product.Create("SKU-007", "Old Name", "Old description", 10.00m, 5);
        var command = new UpdateProductDetailsCommand("New Name", "New description", 20.00m, [1, 2, 3]);
        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(existing);
        var sut = new ProductService(repository);

        await sut.UpdateDetailsAsync(1, command, CancellationToken.None);

        await repository.Received(1).UpdateAsync(
            Arg.Is<Product>(product => product.Sku == "SKU-007" && product.Stock == 5),
            command.RowVersion,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDetailsAsync_StaleRowVersion_ReturnsConcurrencyConflictError()
    {
        var repository = Substitute.For<IProductRepository>();
        var existing = Product.Create("SKU-008", "Name", null, 10.00m, 5);
        var command = new UpdateProductDetailsCommand("New Name", null, 12.00m, [9, 9]);
        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.ConcurrencyConflict);
        var sut = new ProductService(repository);

        var result = await sut.UpdateDetailsAsync(1, command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.ConcurrencyConflict, result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsSuccess()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(Result.Success);
        var sut = new ProductService(repository);

        var result = await sut.DeleteAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_MissingId_ReturnsNotFoundError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.DeleteAsync(99, Arg.Any<CancellationToken>()).Returns((Result)ProductError.NotFound);
        var sut = new ProductService(repository);

        var result = await sut.DeleteAsync(99, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.NotFound, result.Error);
    }

    [Fact]
    public async Task AddStockAsync_ExistingId_ReturnsSuccessWithUpdatedDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var updated = Product.Create("SKU-009", "Name", null, 10.00m, 15);
        repository.AddStockAsync(1, 5, Arg.Any<CancellationToken>()).Returns(updated);
        var sut = new ProductService(repository);

        var result = await sut.AddStockAsync(1, 5, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value.Stock);
    }

    [Fact]
    public async Task AddStockAsync_MissingId_ReturnsNotFoundError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.AddStockAsync(99, 5, Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.NotFound);
        var sut = new ProductService(repository);

        var result = await sut.AddStockAsync(99, 5, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.NotFound, result.Error);
    }

    [Fact]
    public async Task AddStockAsync_ExceedsMaximum_ReturnsStockOverflowError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.AddStockAsync(1, int.MaxValue, Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.StockOverflow);
        var sut = new ProductService(repository);

        var result = await sut.AddStockAsync(1, int.MaxValue, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.StockOverflow, result.Error);
    }

    [Fact]
    public async Task DecrementStockAsync_ExistingId_ReturnsSuccessWithUpdatedDto()
    {
        var repository = Substitute.For<IProductRepository>();
        var updated = Product.Create("SKU-010", "Name", null, 10.00m, 2);
        repository.DecrementStockAsync(1, 3, Arg.Any<CancellationToken>()).Returns(updated);
        var sut = new ProductService(repository);

        var result = await sut.DecrementStockAsync(1, 3, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Stock);
    }

    [Fact]
    public async Task DecrementStockAsync_MissingId_ReturnsNotFoundError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.DecrementStockAsync(99, 3, Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.NotFound);
        var sut = new ProductService(repository);

        var result = await sut.DecrementStockAsync(99, 3, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.NotFound, result.Error);
    }

    [Fact]
    public async Task DecrementStockAsync_QuantityExceedsStock_ReturnsInsufficientStockError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.DecrementStockAsync(1, 100, Arg.Any<CancellationToken>()).Returns((Result<Product>)ProductError.InsufficientStock);
        var sut = new ProductService(repository);

        var result = await sut.DecrementStockAsync(1, 100, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.InsufficientStock, result.Error);
    }

    [Fact]
    public async Task SearchByNameAsync_Always_ReturnsMappedProducts()
    {
        var repository = Substitute.For<IProductRepository>();
        var product = Product.Create("SKU-011", "Widget", null, 9.99m, 4);
        repository.SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Product>)[product]);
        var sut = new ProductService(repository);

        var result = await sut.SearchByNameAsync("widget", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(product.Name, result[0].Name);
    }

    [Fact]
    public async Task SearchByNameAsync_TermWithSurroundingWhitespace_TrimsTermBeforeCallingRepository()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Product>)[]);
        var sut = new ProductService(repository);

        await sut.SearchByNameAsync("  widget  ", CancellationToken.None);

        await repository.Received(1).SearchByNameAsync("widget", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListByStockRangeAsync_Always_ReturnsMappedProducts()
    {
        var repository = Substitute.For<IProductRepository>();
        var product = Product.Create("SKU-012", "Widget", null, 9.99m, 4);
        repository.ListByStockRangeAsync(1, 10, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<Product>)[product]);
        var sut = new ProductService(repository);

        var result = await sut.ListByStockRangeAsync(1, 10, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(product.Stock, result[0].Stock);
    }
}
