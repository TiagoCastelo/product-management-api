using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProductManagement.Application.Products;
using ProductManagement.Infrastructure.Persistence;

namespace ProductManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		builder.AddSqlServerDbContext<ProductDbContext>(
			ProductDbContext.ConnectionName,
			configureDbContextOptions: options => options.UseProductSeeding());
		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		return builder;
	}
}