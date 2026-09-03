namespace ProductCatalog.Domain.Products;

public sealed class ProductDomainException : Exception
{
    public ProductDomainException(string message)
        : base(message)
    {
    }
}
