namespace ProductCatalog.Domain.Products;

public sealed class Product
{
    private Product()
    {
    }

    private Product(
        Guid id,
        string sku,
        string name,
        string? description,
        decimal price,
        string actorId,
        DateTime createdAtUtc)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        Status = ProductStatus.Active;
        CreatedAtUtc = createdAtUtc;
        CreatedBy = actorId;
        ModifiedAtUtc = createdAtUtc;
        ModifiedBy = actorId;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = null!;

    public DateTime ModifiedAtUtc { get; private set; }

    public string ModifiedBy { get; private set; } = null!;

    public long Version { get; private set; }

    public static Product Create(
        Guid id,
        string sku,
        string name,
        string? description,
        decimal price,
        string actorId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ProductDomainException("Product ID is required.");
        }

        var normalizedSku = NormalizeRequired(sku, "SKU").ToUpperInvariant();
        var normalizedName = NormalizeRequired(name, "Name");
        var normalizedDescription = NormalizeOptional(description);
        ValidatePrice(price);
        var normalizedActor = NormalizeActor(actorId);
        ValidateUtc(createdAtUtc);

        return new Product(
            id,
            normalizedSku,
            normalizedName,
            normalizedDescription,
            price,
            normalizedActor,
            createdAtUtc);
    }

    public bool UpdateDetails(
        string name,
        string? description,
        decimal price,
        string actorId,
        DateTime modifiedAtUtc)
    {
        var normalizedName = NormalizeRequired(name, "Name");
        var normalizedDescription = NormalizeOptional(description);
        ValidatePrice(price);

        if (Name == normalizedName && Description == normalizedDescription && Price == price)
        {
            return false;
        }

        var normalizedActor = NormalizeActor(actorId);
        ValidateUtc(modifiedAtUtc);

        Name = normalizedName;
        Description = normalizedDescription;
        Price = price;
        RecordModification(normalizedActor, modifiedAtUtc);
        return true;
    }

    public bool Activate(string actorId, DateTime modifiedAtUtc)
    {
        if (Status == ProductStatus.Active)
        {
            return false;
        }

        var normalizedActor = NormalizeActor(actorId);
        ValidateUtc(modifiedAtUtc);

        Status = ProductStatus.Active;
        RecordModification(normalizedActor, modifiedAtUtc);
        return true;
    }

    public bool Deactivate(string actorId, DateTime modifiedAtUtc)
    {
        if (Status == ProductStatus.Inactive)
        {
            return false;
        }

        var normalizedActor = NormalizeActor(actorId);
        ValidateUtc(modifiedAtUtc);

        Status = ProductStatus.Inactive;
        RecordModification(normalizedActor, modifiedAtUtc);
        return true;
    }

    private void RecordModification(string actorId, DateTime modifiedAtUtc)
    {
        ModifiedBy = actorId;
        ModifiedAtUtc = modifiedAtUtc;
        Version = checked(Version + 1);
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ProductDomainException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeActor(string? actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ProductDomainException("Authenticated user identity is required.");
        }

        return actorId.Trim();
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ProductDomainException("Price cannot be negative.");
        }

        if (decimal.Round(price, 2) != price)
        {
            throw new ProductDomainException("Price cannot have more than two decimal places.");
        }
    }

    private static void ValidateUtc(DateTime timestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ProductDomainException("Audit timestamp must be UTC.");
        }
    }
}
