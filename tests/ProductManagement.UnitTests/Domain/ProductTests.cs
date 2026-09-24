using ProductManagement.Domain.Products;

namespace ProductManagement.UnitTests.Domain;

public class ProductTests
{
    private const string ValidSku = "SKU-001";
    private const string ValidName = "Widget";
    private const string ValidDescription = "A widget";
    private const decimal ValidPrice = 19.99m;
    private const int ValidStock = 10;

    [Fact]
    public void Create_ValidInputs_ReturnsProductWithProvidedValues()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, ValidStock);

        Assert.Equal(ValidSku, product.Sku);
        Assert.Equal(ValidName, product.Name);
        Assert.Equal(ValidDescription, product.Description);
        Assert.Equal(ValidPrice, product.Price);
        Assert.Equal(ValidStock, product.Stock);
    }

    [Fact]
    public void Create_PriceWithInsignificantTrailingZero_IsAccepted()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, 49.990m, ValidStock);

        Assert.Equal(49.990m, product.Price);
    }

    [Fact]
    public void Create_MinimumLengthSku_IsAccepted()
    {
        var product = Product.Create("ABC", ValidName, ValidDescription, ValidPrice, ValidStock);

        Assert.Equal("ABC", product.Sku);
    }

    [Fact]
    public void Create_MaximumLengthSku_IsAccepted()
    {
        var sku = new string('A', 32);

        var product = Product.Create(sku, ValidName, ValidDescription, ValidPrice, ValidStock);

        Assert.Equal(sku, product.Sku);
    }

    [Fact]
    public void Create_MaximumLengthName_IsAccepted()
    {
        var name = new string('a', ProductRules.NameMaxLength);

        var product = Product.Create(ValidSku, name, ValidDescription, ValidPrice, ValidStock);

        Assert.Equal(name, product.Name);
    }

    [Fact]
    public void Create_NullDescription_IsAccepted()
    {
        var product = Product.Create(ValidSku, ValidName, null, ValidPrice, ValidStock);

        Assert.Null(product.Description);
    }

    [Fact]
    public void Create_MaximumLengthDescription_IsAccepted()
    {
        var description = new string('a', ProductRules.DescriptionMaxLength);

        var product = Product.Create(ValidSku, ValidName, description, ValidPrice, ValidStock);

        Assert.Equal(description, product.Description);
    }

    [Fact]
    public void Create_MinimumPrice_IsAccepted()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ProductRules.MinPrice, ValidStock);

        Assert.Equal(ProductRules.MinPrice, product.Price);
    }

    [Fact]
    public void Create_MaximumPrice_IsAccepted()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ProductRules.MaxPrice, ValidStock);

        Assert.Equal(ProductRules.MaxPrice, product.Price);
    }

    [Fact]
    public void Create_ZeroStock_IsAccepted()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, 0);

        Assert.Equal(0, product.Stock);
    }

    [Theory]
    [MemberData(nameof(InvalidSkus))]
    public void Create_InvalidSku_ThrowsArgumentException(string sku)
    {
        Assert.Throws<ArgumentException>(() => Product.Create(sku, ValidName, ValidDescription, ValidPrice, ValidStock));
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Create_InvalidName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => Product.Create(ValidSku, name, ValidDescription, ValidPrice, ValidStock));
    }

    [Fact]
    public void Create_DescriptionExceedsMaxLength_ThrowsArgumentException()
    {
        var description = new string('a', ProductRules.DescriptionMaxLength + 1);

        Assert.Throws<ArgumentException>(() => Product.Create(ValidSku, ValidName, description, ValidPrice, ValidStock));
    }

    [Theory]
    [MemberData(nameof(InvalidPrices))]
    public void Create_InvalidPrice_ThrowsArgumentException(decimal price)
    {
        Assert.ThrowsAny<ArgumentException>(() => Product.Create(ValidSku, ValidName, ValidDescription, price, ValidStock));
    }

    [Fact]
    public void Create_NegativeStock_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, -1));
    }

    [Fact]
    public void UpdateDetails_ValidInputs_UpdatesNameDescriptionAndPrice()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, ValidStock);

        product.UpdateDetails("New Name", "New description", 29.99m);

        Assert.Equal("New Name", product.Name);
        Assert.Equal("New description", product.Description);
        Assert.Equal(29.99m, product.Price);
    }

    [Fact]
    public void UpdateDetails_ValidInputs_DoesNotChangeSkuOrStock()
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, ValidStock);

        product.UpdateDetails("New Name", "New description", 29.99m);

        Assert.Equal(ValidSku, product.Sku);
        Assert.Equal(ValidStock, product.Stock);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void UpdateDetails_InvalidName_ThrowsArgumentException(string name)
    {
        var product = Product.Create(ValidSku, ValidName, ValidDescription, ValidPrice, ValidStock);

        Assert.Throws<ArgumentException>(() => product.UpdateDetails(name, ValidDescription, ValidPrice));
    }

    public static TheoryData<string> InvalidSkus() => new()
    {
        "sku-001",
        "AB",
        new string('A', 33),
        "SKU 001",
        "SKU_001",
    };

    public static TheoryData<string> InvalidNames() => new()
    {
        "   ",
        new string('a', ProductRules.NameMaxLength + 1),
    };

    public static TheoryData<decimal> InvalidPrices() => new()
    {
        0m,
        -1m,
        0.009m,
        1_000_000.01m,
        1.999m,
    };
}
