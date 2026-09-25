using System.Text.RegularExpressions;

namespace ProductManagement.Domain.Products;

public static partial class ProductRules
{
    public const string SkuPattern = "^[A-Z0-9-]{3,32}$";
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const decimal MinPrice = 0.01m;
    public const decimal MaxPrice = 1_000_000.00m;
    public const int MaxPriceDecimals = 2;

    [GeneratedRegex(SkuPattern)]
    public static partial Regex Sku();
}
