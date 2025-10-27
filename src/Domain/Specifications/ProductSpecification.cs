namespace ECommerce.Domain.Specifications;

/// <summary>
/// Specification for filtering products based on various criteria
/// </summary>
public sealed class ProductSpecification
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsOnSale { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? InStock { get; set; }
    public List<string>? Tags { get; set; }
    public string? Brand { get; set; }
    public decimal? MinRating { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Validates if the specification parameters are valid
    /// </summary>
    public bool IsValid()
    {
        if (MinPrice.HasValue && MinPrice.Value < 0)
            return false;

        if (MaxPrice.HasValue && MaxPrice.Value < 0)
            return false;

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
            return false;

        if (MinRating.HasValue && (MinRating.Value < 0 || MinRating.Value > 5))
            return false;

        if (PageNumber < 1 || PageSize < 1 || PageSize > 100)
            return false;

        return true;
    }

    /// <summary>
    /// Creates a specification for featured products
    /// </summary>
    public static ProductSpecification FeaturedProducts(int pageSize = 10)
    {
        return new ProductSpecification
        {
            IsFeatured = true,
            InStock = true,
            PageSize = pageSize,
            SortBy = "CreatedAt",
            SortDescending = true,
        };
    }

    /// <summary>
    /// Creates a specification for products on sale
    /// </summary>
    public static ProductSpecification OnSaleProducts(int pageSize = 20)
    {
        return new ProductSpecification
        {
            IsOnSale = true,
            InStock = true,
            PageSize = pageSize,
            SortBy = "DiscountPercentage",
            SortDescending = true,
        };
    }

    /// <summary>
    /// Creates a specification for low stock products
    /// </summary>
    public static ProductSpecification LowStockProducts()
    {
        return new ProductSpecification
        {
            InStock = true,
            PageSize = 100,
            SortBy = "StockQuantity",
            SortDescending = false,
        };
    }
}
