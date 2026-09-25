using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProductManagement.Domain.Products;

namespace ProductManagement.Infrastructure.Persistence;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public const string IdSequenceName = "ProductIds";
    public const int IdSequenceStart = 100000;
    public const int IdSequenceMin = 100000;
    public const int IdSequenceMax = 999999;

    private const string StockNonNegativeConstraintName = "CK_Products_Stock_NonNegative";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseSequence(IdSequenceName);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(32)
            .IsUnicode(false);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProductRules.NameMaxLength)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(p => p.Description)
            .HasMaxLength(ProductRules.DescriptionMaxLength);

        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.ToTable(t => t.HasCheckConstraint(StockNonNegativeConstraintName, "[Stock] >= 0"));
    }
}
