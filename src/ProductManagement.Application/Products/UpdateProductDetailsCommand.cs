namespace ProductManagement.Application.Products;

public sealed record UpdateProductDetailsCommand(string Name, string? Description, decimal Price, byte[] RowVersion);
