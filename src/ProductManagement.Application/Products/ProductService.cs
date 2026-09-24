using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products;

public sealed class ProductService(IProductRepository repository)
{
    public async Task<IReadOnlyList<ProductDto>> ListAllAsync(CancellationToken cancellationToken) =>
        ToDtos(await repository.ListAllAsync(cancellationToken));

    public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? ProductError.NotFound : product.ToDto();
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(command.Sku, command.Name, command.Description, command.Price, command.Stock);
        var result = await repository.AddAsync(product, cancellationToken);
        return result.IsSuccess ? result.Value.ToDto() : result.Error;
    }

    public async Task<Result<ProductDto>> UpdateDetailsAsync(int id, UpdateProductDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return ProductError.NotFound;
        }

        product.UpdateDetails(command.Name, command.Description, command.Price);
        var result = await repository.UpdateAsync(product, command.RowVersion, cancellationToken);
        return result.IsSuccess ? result.Value.ToDto() : result.Error;
    }

    public Task<Result> DeleteAsync(int id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);

    public async Task<Result<ProductDto>> AddStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var result = await repository.AddStockAsync(id, quantity, cancellationToken);
        return result.IsSuccess ? result.Value.ToDto() : result.Error;
    }

    public async Task<Result<ProductDto>> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var result = await repository.DecrementStockAsync(id, quantity, cancellationToken);
        return result.IsSuccess ? result.Value.ToDto() : result.Error;
    }

    public async Task<IReadOnlyList<ProductDto>> SearchByNameAsync(string term, CancellationToken cancellationToken) =>
        ToDtos(await repository.SearchByNameAsync(term.Trim(), cancellationToken));

    public async Task<IReadOnlyList<ProductDto>> ListByStockRangeAsync(int minimum, int maximum,
        CancellationToken cancellationToken) =>
        ToDtos(await repository.ListByStockRangeAsync(minimum, maximum, cancellationToken));

    private static IReadOnlyList<ProductDto> ToDtos(IReadOnlyList<Product> products) =>
        [.. products.Select(product => product.ToDto())];
}