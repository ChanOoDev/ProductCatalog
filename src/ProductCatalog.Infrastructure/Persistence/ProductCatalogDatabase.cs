using Microsoft.EntityFrameworkCore;

namespace ProductCatalog.Infrastructure.Persistence;

public static class ProductCatalogDatabase
{
    public const string ConnectionStringName = "ProductCatalog";

    public static MySqlServerVersion ServerVersion { get; } = new(new Version(8, 0, 46));
}
