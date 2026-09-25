using ProductManagement.Testing;

using Reqnroll;

namespace ProductManagement.AcceptanceTests.Hooks;

[Binding]
public sealed class AcceptanceTestHooks
{
    private static SqlServerFixture? s_sqlServer;

    public static ProductApiFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task StartInfrastructureAsync()
    {
        s_sqlServer = new SqlServerFixture();
        await s_sqlServer.StartAsync(CancellationToken.None);
        Factory = new ProductApiFactory(s_sqlServer.ConnectionString);
    }

    [AfterTestRun]
    public static async Task StopInfrastructureAsync()
    {
        Factory.Dispose();
        if (s_sqlServer is not null)
        {
            await s_sqlServer.DisposeAsync();
        }
    }
}
