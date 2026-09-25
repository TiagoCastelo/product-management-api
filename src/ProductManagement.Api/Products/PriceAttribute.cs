using System.ComponentModel.DataAnnotations;
using System.Globalization;

using ProductManagement.Domain.Products;

namespace ProductManagement.Api.Products;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PriceAttribute() : ValidationAttribute(ErrorText)
{
    private static readonly string ErrorText = string.Create(
        CultureInfo.InvariantCulture,
        $"Price must be between {ProductRules.MinPrice} and {ProductRules.MaxPrice} with at most {ProductRules.MaxPriceDecimals} decimal places.");

    public override bool IsValid(object? value) =>
        value is not decimal price
        || (price is >= ProductRules.MinPrice and <= ProductRules.MaxPrice
            && decimal.Round(price, ProductRules.MaxPriceDecimals) == price);
}
