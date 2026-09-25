using ProductManagement.AcceptanceTests.Hooks;
using ProductManagement.Application.Products;
using ProductManagement.Testing;

namespace ProductManagement.AcceptanceTests.Support;

public sealed class ProductScenarioContext
{
    private readonly string _token = TestIdentifiers.NewToken();
    private readonly Dictionary<string, ProductDto> _products = [];

    public HttpClient Client { get; } = AcceptanceTestHooks.Factory.CreateClient();

    public HttpResponseMessage LastResponse { get; set; } = null!;

    public IReadOnlyList<ProductDto> LastResults { get; set; } = [];

    public string QualifiedName(string displayName) => $"{_token} {displayName}";

    public void Remember(string displayName, ProductDto product) => _products[displayName] = product;

    public void Forget(string displayName) => _products.Remove(displayName);

    public ProductDto Get(string displayName) => _products[displayName];
}
