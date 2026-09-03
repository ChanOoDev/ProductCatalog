using FluentAssertions;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Domain.UnitTests.Products;

public sealed class ProductUpdateTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 9, 3, 4, 5, 6, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = CreatedAtUtc.AddMinutes(5);

    [Fact]
    public void UpdateDetails_WithChangedValues_UpdatesOnlyEditableFieldsAndAdvancesVersion()
    {
        var product = Create();

        var changed = product.UpdateDetails(" Updated Name ", " Updated description ", 0m, "admin-2", UpdatedAtUtc);

        changed.Should().BeTrue();
        product.Sku.Should().Be("SKU-001");
        product.Name.Should().Be("Updated Name");
        product.Description.Should().Be("Updated description");
        product.Price.Should().Be(0m);
        product.ModifiedBy.Should().Be("admin-2");
        product.ModifiedAtUtc.Should().Be(UpdatedAtUtc);
        product.Version.Should().Be(2);
    }

    [Fact]
    public void UpdateDetails_WithIdenticalValues_IsIdempotent()
    {
        var product = Create();

        var changed = product.UpdateDetails(product.Name, product.Description, product.Price, "admin-2", UpdatedAtUtc);

        changed.Should().BeFalse();
        product.ModifiedBy.Should().Be("admin-1");
        product.ModifiedAtUtc.Should().Be(CreatedAtUtc);
        product.Version.Should().Be(1);
    }

    [Fact]
    public void UpdateDetails_WithInvalidValues_DoesNotPartiallyUpdate()
    {
        var product = Create();

        var act = () => product.UpdateDetails("New Name", "New description", -1m, "admin-2", UpdatedAtUtc);

        act.Should().Throw<ProductDomainException>();
        product.Name.Should().Be("Office Chair");
        product.Description.Should().BeNull();
        product.Price.Should().Be(99.99m);
        product.Version.Should().Be(1);
    }

    [Fact]
    public void Sku_HasNoPublicSetter()
    {
        typeof(Product).GetProperty(nameof(Product.Sku))!.SetMethod.Should().NotBeNull();
        typeof(Product).GetProperty(nameof(Product.Sku))!.SetMethod!.IsPublic.Should().BeFalse();
        typeof(Product).GetMethod("UpdateSku").Should().BeNull();
    }

    private static Product Create() =>
        Product.Create(Guid.NewGuid(), "SKU-001", "Office Chair", null, 99.99m, "admin-1", CreatedAtUtc);
}
