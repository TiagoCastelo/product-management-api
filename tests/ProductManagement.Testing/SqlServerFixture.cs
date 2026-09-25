using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ProductManagement.Infrastructure.Persistence;

using Testcontainers.MsSql;

namespace ProductManagement.Testing;

public sealed class SqlServerFixture : IAsyncDisposable
{
    public const string ImageName = "mcr.microsoft.com/mssql/server:2022-latest";

    private const string DefaultDatabaseName = "productsdb";

    private readonly MsSqlContainer _container = new MsSqlBuilder(ImageName).Build();
    private readonly SemaphoreSlim _provisioningLock = new(1, 1);

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _container.StartAsync(cancellationToken);
        ConnectionString = await CreateDatabaseAsync(DefaultDatabaseName, cancellationToken);
    }

    public async Task<string> CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        var connectionString = BuildConnectionString(databaseName);

        await _provisioningLock.WaitAsync(cancellationToken);
        try
        {
            using var factory = new ProductApiFactory(connectionString);
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            await context.Database.MigrateAsync(cancellationToken);
        }
        finally
        {
            _provisioningLock.Release();
        }

        return connectionString;
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    private string BuildConnectionString(string databaseName) =>
        new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = databaseName }.ConnectionString;
}
