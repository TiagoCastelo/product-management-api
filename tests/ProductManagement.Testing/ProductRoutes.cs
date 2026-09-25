namespace ProductManagement.Testing;

public static class ProductRoutes
{
    public const string Products = "/api/products";

    public static Uri Product(int id) => new($"{Products}/{id}", UriKind.Relative);

    public static Uri DecrementStock(int id, string quantity) => new($"{Products}/{id}/decrement-stock/{quantity}", UriKind.Relative);

    public static Uri AddStock(int id, string quantity) => new($"{Products}/{id}/add-to-stock/{quantity}", UriKind.Relative);

    public static Uri Search(string term) => new($"{Products}/search?name={Uri.EscapeDataString(term)}", UriKind.Relative);

    public static Uri StockLevel(string query) => new($"{Products}/stock-level?{query}", UriKind.Relative);
}
