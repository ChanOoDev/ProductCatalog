using FluentAssertions;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Domain.UnitTests.Products;

public sealed class ProductLifecycleTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 9, 3, 4, 5, 6, DateTimeKind.Utc);

    [Fact]
    public void Deactivate_WhenActive_ChangesStatusAndAdvancesAuditAndVersion()
    {
        var product = Create();
        var changedAt = CreatedAtUtc.AddMinutes(1);

        var changed = product.Deactivate("admin-2", changedAt);

        changed.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Inactive);
        product.ModifiedBy.Should().Be("admin-2");
        product.ModifiedAtUtc.Should().Be(changedAt);
        product.Version.Should().Be(2);
    }

    [Fact]
    public void Deactivate_WhenInactive_IsIdempotent()
    {
        var product = Create();
        product.Deactivate("admin-2", CreatedAtUtc.AddMinutes(1));
        var version = product.Version;
        var modifiedAt = product.ModifiedAtUtc;

        var changed = product.Deactivate("admin-3", CreatedAtUtc.AddMinutes(2));

        changed.Should().BeFalse();
        product.Version.Should().Be(version);
        product.ModifiedAtUtc.Should().Be(modifiedAt);
        product.ModifiedBy.Should().Be("admin-2");
    }

    [Fact]
    public void Activate_WhenInactive_ChangesStatusAndAdvancesAuditAndVersion()
    {
        var product = Create();
        product.Deactivate("admin-2", CreatedAtUtc.AddMinutes(1));

        var changed = product.Activate("admin-3", CreatedAtUtc.AddMinutes(2));

        changed.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
        product.ModifiedBy.Should().Be("admin-3");
        product.Version.Should().Be(3);
    }

    [Fact]
    public void Activate_WhenActive_IsIdempotent()
    {
        var product = Create();

        var changed = product.Activate("admin-2", CreatedAtUtc.AddMinutes(1));

        changed.Should().BeFalse();
        product.ModifiedBy.Should().Be("admin-1");
        product.ModifiedAtUtc.Should().Be(CreatedAtUtc);
        product.Version.Should().Be(1);
    }

    private static Product Create() =>
        Product.Create(Guid.NewGuid(), "SKU-001", "Office Chair", null, 99.99m, "admin-1", CreatedAtUtc);
}
