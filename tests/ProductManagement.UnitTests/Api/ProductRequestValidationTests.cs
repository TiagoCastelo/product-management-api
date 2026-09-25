using System.Globalization;
using System.Reflection;

using ProductManagement.Api.Products;

namespace ProductManagement.UnitTests.Api;

public class ProductRequestValidationTests
{
    public static TheoryData<string, Type> CulturesAndRequests => new()
    {
        { "en-US", typeof(CreateProductRequest) },
        { "pt-PT", typeof(CreateProductRequest) },
        { "de-DE", typeof(CreateProductRequest) },
        { "fr-FR", typeof(CreateProductRequest) },
        { "en-US", typeof(UpdateProductRequest) },
        { "pt-PT", typeof(UpdateProductRequest) },
        { "de-DE", typeof(UpdateProductRequest) },
        { "fr-FR", typeof(UpdateProductRequest) },
    };

    [Theory]
    [MemberData(nameof(CulturesAndRequests))]
    public void PriceAttribute_ValidPrices_AreValidInAnyCulture(string culture, Type requestType)
    {
        var results = UnderCulture(culture, () => AreValid(requestType, 0.01m, 49.99m, 49.990m, 1_000_000.00m));

        Assert.All(results, Assert.True);
    }

    [Theory]
    [MemberData(nameof(CulturesAndRequests))]
    public void PriceAttribute_InvalidPrices_AreInvalidInAnyCulture(string culture, Type requestType)
    {
        var results = UnderCulture(culture, () => AreValid(requestType, 0.009m, 1_000_000.01m, 1.999m, -1m));

        Assert.All(results, Assert.False);
    }

    private static List<bool> AreValid(Type requestType, params decimal[] prices)
    {
        var freshAttribute = requestType.GetProperty("Price")!.GetCustomAttribute<PriceAttribute>()!;
        return [.. prices.Select(price => freshAttribute.IsValid(price))];
    }

    private static T UnderCulture<T>(string culture, Func<T> action)
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
