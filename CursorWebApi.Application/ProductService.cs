using CursorWebApi.Domain;
using CursorWebApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CursorWebApi.Application;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Retrieving all products");
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, string category, int stockQuantity = 0)
    {
        _logger.LogInformation("Creating new product: {ProductName}, Price: {Price}, Category: {Category}",
            name, price, category);

        // Business Rule: Use domain factory method
        var product = Product.Create(name, price, category, stockQuantity);

        await _repository.AddAsync(product);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        return product;
    }

    public async Task UpdateProductAsync(int id, string name, decimal price, string category)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
            throw new ProductNotFoundException(id);

                // Business Rule: Use domain methods for validation
        existingProduct.UpdateName(name);
        existingProduct.UpdatePrice(price);
        existingProduct.UpdateCategory(category);

        await _repository.UpdateAsync(existingProduct);

        _logger.LogInformation("Product updated successfully: {ProductId}", id);
    }

    public async Task DeleteProductAsync(int id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        // Business Rule: Check if product can be deleted
        if (product.StockQuantity > 0)
        {
            _logger.LogWarning("Attempting to delete product with stock: {ProductId}, Stock: {Stock}",
                id, product.StockQuantity);
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        _logger.LogInformation("Retrieving products by category: {Category}", category);

        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("Category cannot be empty.");

        var allProducts = await _repository.GetAllAsync();
        return allProducts.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 5)
    {
        _logger.LogInformation("Retrieving products with low stock (threshold: {Threshold})", threshold);

        if (threshold < 0)
            throw new ValidationException("Stock threshold cannot be negative.");

        var allProducts = await _repository.GetAllAsync();
        return allProducts.Where(p => p.IsLowStock(threshold));
    }

    public async Task<decimal> CalculateProductDiscountAsync(int productId, decimal discountPercentage)
    {
        _logger.LogInformation("Calculating discount for product {ProductId} with {DiscountPercentage}% discount",
            productId, discountPercentage);

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            throw new ProductNotFoundException(productId);

        // Business Rule: Use domain method for calculation
        var discountedPrice = product.CalculateDiscountedPrice(discountPercentage);

        _logger.LogInformation("Discounted price calculated: {OriginalPrice} -> {DiscountedPrice}",
            product.Price, discountedPrice);

        return discountedPrice;
    }
}
