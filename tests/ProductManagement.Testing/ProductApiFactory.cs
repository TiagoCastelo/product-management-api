using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using ProductManagement.Infrastructure.Persistence;

namespace ProductManagement.Testing;

public sealed class ProductApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting($"ConnectionStrings:{ProductDbContext.ConnectionName}", connectionString);
}
