using FluentAssertions;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Domain.UnitTests.Products;

public sealed class ProductCreationTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 9, 3, 4, 5, 6, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidValues_NormalizesAndCreatesActiveProduct()
    {
        var id = Guid.NewGuid();

        var product = Product.Create(id, " sku-001 ", " Office Chair ", " Ergonomic ", 0m, "admin-1", CreatedAtUtc);

        product.Id.Should().Be(id);
        product.Sku.Should().Be("SKU-001");
        product.Name.Should().Be("Office Chair");
        product.Description.Should().Be("Ergonomic");
        product.Price.Should().Be(0m);
        product.Status.Should().Be(ProductStatus.Active);
        product.CreatedAtUtc.Should().Be(CreatedAtUtc);
        product.ModifiedAtUtc.Should().Be(CreatedAtUtc);
        product.CreatedBy.Should().Be("admin-1");
        product.ModifiedBy.Should().Be("admin-1");
        product.Version.Should().Be(1);
    }

    [Fact]
    public void Create_WithWhitespaceDescription_StoresNull()
    {
        var product = Create(description: "   ");

        product.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingSku_Throws(string sku)
    {
        var act = () => Create(sku: sku);

        act.Should().Throw<ProductDomainException>().WithMessage("SKU is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingName_Throws(string name)
    {
        var act = () => Create(name: name);

        act.Should().Throw<ProductDomainException>().WithMessage("Name is required.");
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Create(price: -0.01m);

        act.Should().Throw<ProductDomainException>().WithMessage("Price cannot be negative.");
    }

    [Fact]
    public void Create_WithMoreThanTwoDecimalPlaces_Throws()
    {
        var act = () => Create(price: 1.001m);

        act.Should().Throw<ProductDomainException>().WithMessage("Price cannot have more than two decimal places.");
    }

    [Fact]
    public void Create_WithNonUtcTimestamp_Throws()
    {
        var act = () => Create(createdAtUtc: DateTime.SpecifyKind(CreatedAtUtc, DateTimeKind.Local));

        act.Should().Throw<ProductDomainException>().WithMessage("Audit timestamp must be UTC.");
    }

    [Fact]
    public void Create_WithEmptyActor_Throws()
    {
        var act = () => Create(actorId: " ");

        act.Should().Throw<ProductDomainException>().WithMessage("Authenticated user identity is required.");
    }

    private static Product Create(
        string sku = "SKU-001",
        string name = "Office Chair",
        string? description = null,
        decimal price = 99.99m,
        string actorId = "admin-1",
        DateTime? createdAtUtc = null) =>
        Product.Create(Guid.NewGuid(), sku, name, description, price, actorId, createdAtUtc ?? CreatedAtUtc);
}
