namespace ProductManagement.Testing;

public static class ProductRoutes
{
    public const string Products = "/api/products";

    public static Uri Product(int id) => new($"/api/products/{id}", UriKind.Relative);

    public static Uri DecrementStock(int id, string quantity) => new($"/api/products/{id}/decrement-stock/{quantity}", UriKind.Relative);

    public static Uri AddStock(int id, string quantity) => new($"/api/products/{id}/add-to-stock/{quantity}", UriKind.Relative);

    public static Uri Search(string term) => new($"/api/products/search?name={Uri.EscapeDataString(term)}", UriKind.Relative);

    public static Uri StockLevel(string query) => new($"/api/products/stock-level?{query}", UriKind.Relative);
}
