using ProductManagement.IntegrationTests;
using ProductManagement.Testing;

[assembly: AssemblyFixture(typeof(SqlServerAssemblyFixture))]

namespace ProductManagement.IntegrationTests;

public sealed class SqlServerAssemblyFixture : IAsyncLifetime
{
    public SqlServerFixture SqlServer { get; } = new();

    public ProductApiFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await SqlServer.StartAsync(CancellationToken.None);
        Factory = new ProductApiFactory(SqlServer.ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        Factory.Dispose();
        await SqlServer.DisposeAsync();
    }
}
