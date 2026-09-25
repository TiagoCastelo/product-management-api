using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Sku, product.Name, product.Description, product.Price, product.Stock, product.RowVersion);
}
