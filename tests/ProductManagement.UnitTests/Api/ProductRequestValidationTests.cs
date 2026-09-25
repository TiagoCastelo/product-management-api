using System.ComponentModel.DataAnnotations;
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
    public void PriceRange_PricesWithinBounds_AreValidInAnyCulture(string culture, Type requestType)
    {
        var results = UnderCulture(culture, () => AreValid(requestType, 0.01m, 49.99m, 1_000_000.00m));

        Assert.All(results, Assert.True);
    }

    [Theory]
    [MemberData(nameof(CulturesAndRequests))]
    public void PriceRange_PricesOutsideBounds_AreInvalidInAnyCulture(string culture, Type requestType)
    {
        var results = UnderCulture(culture, () => AreValid(requestType, 0.009m, 1_000_000.01m));

        Assert.All(results, Assert.False);
    }

    private static List<bool> AreValid(Type requestType, params decimal[] prices)
    {
        var freshAttribute = requestType.GetProperty("Price")!.GetCustomAttribute<RangeAttribute>()!;
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
