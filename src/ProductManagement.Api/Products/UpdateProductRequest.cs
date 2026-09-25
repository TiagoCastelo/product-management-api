using System.ComponentModel.DataAnnotations;

using ProductManagement.Domain.Products;

namespace ProductManagement.Api.Products;

public sealed record UpdateProductRequest(
    [property: Required, StringLength(ProductRules.NameMaxLength)] string? Name,
    [property: StringLength(ProductRules.DescriptionMaxLength)] string? Description,
    [property: Required, Range(typeof(decimal), "0.01", "1000000.00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal? Price,
    [property: Required] byte[]? RowVersion);
