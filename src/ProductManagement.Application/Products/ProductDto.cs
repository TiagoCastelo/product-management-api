namespace ProductManagement.Application.Products;

public sealed record ProductDto(int Id, string Sku, string Name, string? Description, decimal Price, int Stock,
	byte[] RowVersion);