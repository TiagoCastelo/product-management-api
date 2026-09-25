using Microsoft.Extensions.DependencyInjection;

using ProductManagement.Application.Products;

namespace ProductManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services.AddScoped<ProductService>();
}
