using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductManagement.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
	public ProductDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        
		optionsBuilder.UseSqlServer("Server=fake;Database=fake;Trusted_Connection=True;");

		return new ProductDbContext(optionsBuilder.Options);
	}
}