namespace ProductManagement.Application.Products;

public sealed record CreateProductCommand(string Sku, string Name, string? Description, decimal Price, int Stock);
