using System.ComponentModel.DataAnnotations;

using ProductManagement.Domain.Products;

namespace ProductManagement.Api.Products;

public sealed record CreateProductRequest(
	[property: Required, RegularExpression(ProductRules.SkuPattern)] string? Sku,
	[property: Required, StringLength(ProductRules.NameMaxLength)] string? Name,
	[property: StringLength(ProductRules.DescriptionMaxLength)] string? Description,
	[property: Required, Range(typeof(decimal), "0.01", "1000000.00")] decimal? Price,
	[property: Required, Range(0, int.MaxValue)] int? Stock);