using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.Hooks;

[Binding]
public sealed class AcceptanceTestHooks
{
    private static SqlServerFixture? _sqlServer;

    public static ProductApiFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task StartInfrastructureAsync()
    {
        _sqlServer = new SqlServerFixture();
        await _sqlServer.StartAsync(CancellationToken.None);
        Factory = new ProductApiFactory(_sqlServer.ConnectionString);
    }

    [AfterTestRun]
    public static async Task StopInfrastructureAsync()
    {
        Factory.Dispose();
        if (_sqlServer is not null)
        {
            await _sqlServer.DisposeAsync();
        }
    }
}
