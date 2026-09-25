namespace ProductManagement.Testing;

public static class TestIdentifiers
{
    public static string NewSku() => $"IT-{Guid.NewGuid():N}"[..32].ToUpperInvariant();

    public static string NewToken() => Guid.NewGuid().ToString("N");
}
