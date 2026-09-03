using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Persistence;
using Testcontainers.MySql;

namespace ProductCatalog.Api.IntegrationTests.Infrastructure;

public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer container = new MySqlBuilder("mysql:8.0.46")
        .WithDatabase("product_catalog_tests")
        .WithUsername("catalog_test")
        .WithPassword("catalog_test_password")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(ConnectionString, ProductCatalogDatabase.ServerVersion)
            .Options;
        return new ApplicationDbContext(options);
    }
}
