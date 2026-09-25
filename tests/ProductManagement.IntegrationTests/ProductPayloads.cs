using ProductManagement.Testing;

namespace ProductManagement.IntegrationTests;

internal static class ProductPayloads
{
    public static Dictionary<string, object?> ValidCreate(string? sku = null) => new()
    {
        ["sku"] = sku ?? TestIdentifiers.NewSku(),
        ["name"] = "Test Product",
        ["description"] = "A test product",
        ["price"] = 9.99m,
        ["stock"] = 5,
    };

    public static Dictionary<string, object?> ValidUpdate(byte[] rowVersion) => new()
    {
        ["name"] = "Updated Product",
        ["description"] = "Updated description",
        ["price"] = 19.99m,
        ["rowVersion"] = rowVersion,
    };
}
