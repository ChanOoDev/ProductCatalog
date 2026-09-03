using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProductCatalog.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configurationDirectory = FindConfigurationDirectory();
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(ProductCatalogDatabase.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Configure ConnectionStrings:{ProductCatalogDatabase.ConnectionStringName} or " +
                $"ConnectionStrings__{ProductCatalogDatabase.ConnectionStringName} to generate or inspect migrations.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, ProductCatalogDatabase.ServerVersion)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string FindConfigurationDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidates = new[]
            {
                directory.FullName,
                Path.Combine(directory.FullName, "ProductCatalog.Api"),
                Path.Combine(directory.FullName, "src", "ProductCatalog.Api")
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                {
                    return candidate;
                }
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate ProductCatalog.Api appsettings.json. Run the EF command from the repository root " +
            "or the API project directory.");
    }
}
