using System.Linq.Expressions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using ProductManagement.Application.Products;
using ProductManagement.Domain.Products;

namespace ProductManagement.Infrastructure.Persistence;

public sealed class ProductRepository(ProductDbContext context) : IProductRepository
{
    private const int UniqueIndexViolation = 2601;
    private const int SequenceExhausted = 11728;

    public async Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken) =>
        await OrderedProducts().ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Result<Product>> AddAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Add(product);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return product;
        }
        catch (DbUpdateException ex) when (TryTranslate(ex, out var error))
        {
            context.Entry(product).State = EntityState.Detached;
            return error;
        }
    }

    public async Task<Result<Product>> UpdateAsync(Product product, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        context.Entry(product).Property(p => p.RowVersion).OriginalValue = expectedRowVersion;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return product;
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await context.Products.AsNoTracking().AnyAsync(p => p.Id == product.Id, cancellationToken);
            context.Entry(product).State = EntityState.Detached;
            return exists ? ProductError.ConcurrencyConflict : ProductError.NotFound;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var rows = await context.Products.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
        return rows > 0 ? Result.Success : ProductError.NotFound;
    }

    public async Task<Result<Product>> AddStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var rows = await context.Products
            .Where(p => p.Id == id && p.Stock <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Stock, p => p.Stock + quantity), cancellationToken);

        if (rows == 1)
        {
            return await context.Products.AsNoTracking().FirstAsync(p => p.Id == id, cancellationToken);
        }

        var exists = await context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
        return exists ? ProductError.StockOverflow : ProductError.NotFound;
    }

    public async Task<Result<Product>> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var rows = await context.Products
            .Where(p => p.Id == id && p.Stock >= quantity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Stock, p => p.Stock - quantity), cancellationToken);

        if (rows == 1)
        {
            return await context.Products.AsNoTracking().FirstAsync(p => p.Id == id, cancellationToken);
        }

        var exists = await context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
        return exists ? ProductError.InsufficientStock : ProductError.NotFound;
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string term, CancellationToken cancellationToken) =>
        await OrderedProducts(p => p.Name.Contains(term)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> ListByStockRangeAsync(int minimum, int maximum, CancellationToken cancellationToken) =>
        await OrderedProducts(p => p.Stock >= minimum && p.Stock <= maximum).ToListAsync(cancellationToken);

    private IOrderedQueryable<Product> OrderedProducts() =>
        context.Products.AsNoTracking().OrderBy(p => p.Name).ThenBy(p => p.Id);

    private IOrderedQueryable<Product> OrderedProducts(Expression<Func<Product, bool>> predicate) =>
        context.Products.AsNoTracking().Where(predicate).OrderBy(p => p.Name).ThenBy(p => p.Id);

    private static bool TryTranslate(DbUpdateException exception, out ProductError error)
    {
        if (exception.InnerException is SqlException { Number: UniqueIndexViolation })
        {
            error = ProductError.DuplicateSku;
            return true;
        }

        if (exception.InnerException is SqlException { Number: SequenceExhausted })
        {
            error = ProductError.IdSpaceExhausted;
            return true;
        }

        error = default;
        return false;
    }
}
