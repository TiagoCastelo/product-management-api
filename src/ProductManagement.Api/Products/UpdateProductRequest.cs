using System.ComponentModel.DataAnnotations;

using ProductManagement.Domain.Products;

namespace ProductManagement.Api.Products;

public sealed record UpdateProductRequest(
    [property: Required, StringLength(ProductRules.NameMaxLength)] string? Name,
    [property: StringLength(ProductRules.DescriptionMaxLength)] string? Description,
    [property: Required, Price] decimal? Price,
    [property: Required] byte[]? RowVersion);
