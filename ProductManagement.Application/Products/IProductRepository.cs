using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products;

public interface IProductRepository
{
	Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken);

	Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

	Task<Result<Product>> AddAsync(Product product, CancellationToken cancellationToken);

	Task<Result<Product>> UpdateAsync(Product product, byte[] expectedRowVersion, CancellationToken cancellationToken);

	Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);

	Task<Result<Product>> AddStockAsync(int id, int quantity, CancellationToken cancellationToken);

	Task<Result<Product>> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken);

	Task<IReadOnlyList<Product>> SearchByNameAsync(string term, CancellationToken cancellationToken);

	Task<IReadOnlyList<Product>> ListByStockRangeAsync(int minimum, int maximum, CancellationToken cancellationToken);
}