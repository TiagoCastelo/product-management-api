namespace ProductManagement.Domain.Products;

public sealed class Product
{
    public int Id { get; private set; }

    public string Sku { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public byte[] RowVersion { get; private set; }

    private Product()
    {
        Sku = string.Empty;
        Name = string.Empty;
        RowVersion = [];
    }

    private Product(string sku, string name, string? description, decimal price, int stock)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        RowVersion = [];
    }

    private static readonly decimal ZeroAtPriceScale = new(0, 0, 0, false, ProductRules.MaxPriceDecimals);

    public static Product Create(string sku, string name, string? description, decimal price, int stock)
    {
        ValidateSku(sku);
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        ValidateStock(stock);
        return new Product(sku, name, description, WithPriceScale(price), stock);
    }

    public void UpdateDetails(string name, string? description, decimal price)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        Name = name;
        Description = description;
        Price = WithPriceScale(price);
    }

    private static decimal WithPriceScale(decimal price) => decimal.Round(price, ProductRules.MaxPriceDecimals) + ZeroAtPriceScale;

    private static void ValidateSku(string sku)
    {
        ArgumentNullException.ThrowIfNull(sku);
        if (!ProductRules.Sku().IsMatch(sku))
        {
            throw new ArgumentException($"Sku must match pattern '{ProductRules.SkuPattern}'.", nameof(sku));
        }
    }

    private static void ValidateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty or whitespace.", nameof(name));
        }

        if (name.Length > ProductRules.NameMaxLength)
        {
            throw new ArgumentException($"Name must not exceed {ProductRules.NameMaxLength} characters.", nameof(name));
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (description is { Length: > ProductRules.DescriptionMaxLength })
        {
            throw new ArgumentException($"Description must not exceed {ProductRules.DescriptionMaxLength} characters.",
                nameof(description));
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price is < ProductRules.MinPrice or > ProductRules.MaxPrice)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, $"Price must be between {ProductRules.MinPrice} and" +
                                                                        $" {ProductRules.MaxPrice}.");
        }

        if (decimal.Round(price, ProductRules.MaxPriceDecimals) != price)
        {
            throw new ArgumentException($"Price must have at most {ProductRules.MaxPriceDecimals} decimal places.", nameof(price));
        }
    }

    private static void ValidateStock(int stock)
    {
        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock), stock, "Stock must not be negative.");
        }
    }
}
