using Microsoft.EntityFrameworkCore;

using ProductManagement.Domain.Products;

namespace ProductManagement.Infrastructure.Persistence;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public const string ConnectionName = "productsdb";

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>(ProductConfiguration.IdSequenceName)
            .StartsAt(ProductConfiguration.IdSequenceStart)
            .HasMin(ProductConfiguration.IdSequenceMin)
            .HasMax(ProductConfiguration.IdSequenceMax)
            .IsCyclic(false);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}
