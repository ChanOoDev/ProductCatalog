using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Api.IntegrationTests.Infrastructure;

public sealed class PersistenceTests(MySqlFixture fixture) : IClassFixture<MySqlFixture>
{
    [Fact]
    public async Task Migration_creates_products_table()
    {
        await using var context = fixture.CreateContext();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Products'";

        Convert.ToInt32(await command.ExecuteScalarAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Product_round_trips_with_audit_and_version()
    {
        var product = Product.Create(Guid.NewGuid(), " sku-100 ", "Desk", "Standing desk", 149.95m,
            "admin@example.test", DateTime.UtcNow);
        await using (var writeContext = fixture.CreateContext())
        {
            writeContext.Products.Add(product);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = fixture.CreateContext();
        var stored = await readContext.Products.SingleAsync(value => value.Id == product.Id);
        stored.Sku.Should().Be("SKU-100");
        stored.Price.Should().Be(149.95m);
        stored.Version.Should().Be(1);
        stored.ModifiedBy.Should().Be("admin@example.test");
        stored.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Unique_index_rejects_case_variant_sku()
    {
        var now = DateTime.UtcNow;
        await using var context = fixture.CreateContext();
        context.Products.Add(Product.Create(Guid.NewGuid(), "unique-sku", "First", null, 1m, "tester", now));
        await context.SaveChangesAsync();

        context.Products.Add(Product.Create(Guid.NewGuid(), "UNIQUE-SKU", "Second", null, 2m, "tester", now));
        var save = () => context.SaveChangesAsync();

        await save.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Concurrency_token_rejects_a_stale_update_without_overwrite()
    {
        var id = Guid.NewGuid();
        await using (var seedContext = fixture.CreateContext())
        {
            seedContext.Products.Add(Product.Create(id, "concurrent-sku", "Original", null, 10m,
                "tester", DateTime.UtcNow));
            await seedContext.SaveChangesAsync();
        }

        await using var firstContext = fixture.CreateContext();
        await using var staleContext = fixture.CreateContext();
        var first = await firstContext.Products.SingleAsync(value => value.Id == id);
        var stale = await staleContext.Products.SingleAsync(value => value.Id == id);
        first.UpdateDetails("First update", null, 11m, "first-admin", DateTime.UtcNow);
        stale.UpdateDetails("Stale update", null, 12m, "second-admin", DateTime.UtcNow);

        await firstContext.SaveChangesAsync();
        var staleSave = () => staleContext.SaveChangesAsync();
        await staleSave.Should().ThrowAsync<DbUpdateConcurrencyException>();

        await using var verifyContext = fixture.CreateContext();
        var stored = await verifyContext.Products.SingleAsync(value => value.Id == id);
        stored.Name.Should().Be("First update");
        stored.Version.Should().Be(2);
    }
}
