using System.Net;
using System.Net.Http.Json;

using ProductManagement.Application.Products;

namespace ProductManagement.IntegrationTests;

internal static class ProductTestClient
{
    public static async Task<ProductDto> CreateAsync(HttpClient client, Action<Dictionary<string, object?>>? customize, CancellationToken cancellationToken)
    {
        var payload = ProductPayloads.ValidCreate();
        customize?.Invoke(payload);
        var response = await client.PostAsJsonAsync("/api/products", payload, cancellationToken);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return product!;
    }
}
