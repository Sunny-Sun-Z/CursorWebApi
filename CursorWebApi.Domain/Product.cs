using CursorWebApi.Domain.Exceptions;

namespace CursorWebApi.Domain;

public class Product
{
    // Public setter needed for Infrastructure layer to set ID during persistence
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    // Private constructor for EF Core
    private Product() { }

    // Factory method with business rules
    public static Product Create(string name, decimal price, string category, int stockQuantity = 0)
    {
        // Business Rule 1: Name validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Product name is required.");

        if (name.Length < 2)
            throw new ValidationException("Product name must be at least 2 characters long.");

        if (name.Length > 100)
            throw new ValidationException("Product name cannot exceed 100 characters.");

        // Business Rule 2: Price validation
        if (price < 0)
            throw new ValidationException("Product price cannot be negative.");

        if (price > 10000)
            throw new ValidationException("Product price cannot exceed $10,000.");

        // Business Rule 3: Category validation
        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("Product category is required.");

        // Business Rule 4: Stock validation
        if (stockQuantity < 0)
            throw new ValidationException("Stock quantity cannot be negative.");

        return new Product
        {
            Name = name.Trim(),
            Price = price,
            Category = category.Trim(),
            StockQuantity = stockQuantity
        };
    }

    // Business Rule 5: Update methods with validation
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ValidationException("Product name is required.");

        if (newName.Length < 2)
            throw new ValidationException("Product name must be at least 2 characters long.");

        if (newName.Length > 100)
            throw new ValidationException("Product name cannot exceed 100 characters.");

        Name = newName.Trim();
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ValidationException("Product price cannot be negative.");

        if (newPrice > 10000)
            throw new ValidationException("Product price cannot exceed $10,000.");

        Price = newPrice;
    }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ValidationException("Stock quantity cannot be negative.");

        StockQuantity = newQuantity;
    }

    public void UpdateCategory(string newCategory)
    {
        if (string.IsNullOrWhiteSpace(newCategory))
            throw new ValidationException("Product category is required.");

        Category = newCategory.Trim();
    }

    // Business Rule 6: Business methods
    public bool IsInStock() => StockQuantity > 0;

    public bool IsLowStock(int threshold = 5) => StockQuantity <= threshold;

    public decimal CalculateDiscountedPrice(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ValidationException("Discount percentage must be between 0 and 100.");

        return Price * (1 - discountPercentage / 100);
    }

    // Business Rule 7: Domain events (for future use)
    public bool IsExpensive() => Price > 1000;
}
