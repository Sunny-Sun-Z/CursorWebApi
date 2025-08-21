using CursorWebApi.Domain;
using CursorWebApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using CursorWebApi.Domain.Messaging;


namespace CursorWebApi.Application;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _cache;

    private readonly IDistributedCache _distributedCache;
    private readonly IMessagePublisher _publisher;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger, 
        IMemoryCache cache, IDistributedCache distributedCache, IMessagePublisher publisher)
    {
        _repository = repository;
        _logger = logger;
        _cache = cache;
        _distributedCache = distributedCache;
        _publisher = publisher;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        const string cacheKey = "all_products";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Product>? cachedProducts))
        {
            _logger.LogInformation("Retrieved all products from cache");
            return cachedProducts!;
        }

        _logger.LogInformation("Retrieving all products from repository");
        var products = await _repository.GetAllAsync();

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

        _cache.Set(cacheKey, products, cacheOptions);
        _logger.LogInformation("Cached {ProductCount} products", products.Count());

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var cacheKey = $"product_{id}";

        if (_cache.TryGetValue(cacheKey, out Product? cachedProduct))
        {
            _logger.LogInformation("Retrieved product {ProductId} from cache", id);
            return cachedProduct;
        }

        _logger.LogInformation("Retrieving product with ID: {ProductId} from repository", id);
        var product = await _repository.GetByIdAsync(id);

        if (product != null)
        {
            // Cache for 10 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(cacheKey, product, cacheOptions);
            _logger.LogInformation("Cached product {ProductId}", id);
        }

        return product;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, string category, int stockQuantity = 0)
    {
        _logger.LogInformation("Creating new product: {ProductName}, Price: {Price}, Category: {Category}",
            name, price, category);

        // Business Rule: Use domain factory method
        var product = Product.Create(name, price, category, stockQuantity);
        
        await _publisher.PublishAsync("products", product);
        await _repository.AddAsync(product);

        // Invalidate related caches
        InvalidateProductCaches(product.Id, category);

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

        // Invalidate related caches
        InvalidateProductCaches(id, category);

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

        // Invalidate related caches
        InvalidateProductCaches(id, product.Category);

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        _logger.LogInformation("Retrieving products by category: {Category}", category);

        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("Category cannot be empty.");

        var cacheKey = $"category_{category.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Product>? cachedProducts))
        {
            _logger.LogInformation("Retrieved products for category '{Category}' from cache", category);
            return cachedProducts!;
        }

        _logger.LogInformation("Retrieving products by category: {Category} from repository", category);
        var allProducts = await _repository.GetAllAsync();
        var categoryProducts = allProducts.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        _cache.Set(cacheKey, categoryProducts, cacheOptions);
        _logger.LogInformation("Cached {ProductCount} products for category '{Category}'", categoryProducts.Count, category);

        return categoryProducts;
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 5)
    {
        _logger.LogInformation("Retrieving products with low stock (threshold: {Threshold})", threshold);

        if (threshold < 0)
            throw new ValidationException("Stock threshold cannot be negative.");

        var cacheKey = $"low_stock_{threshold}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Product>? cachedProducts))
        {
            _logger.LogInformation("Retrieved low stock products (threshold: {Threshold}) from cache", threshold);
            return cachedProducts!;
        }

        _logger.LogInformation("Retrieving products with low stock (threshold: {Threshold}) from repository", threshold);
        var allProducts = await _repository.GetAllAsync();
        var lowStockProducts = allProducts.Where(p => p.IsLowStock(threshold)).ToList();

        // Cache for 2 minutes (low stock changes frequently)
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, lowStockProducts, cacheOptions);
        _logger.LogInformation("Cached {ProductCount} low stock products (threshold: {Threshold})", lowStockProducts.Count, threshold);

        return lowStockProducts;
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

    // Cache invalidation helper method
    private void InvalidateProductCaches(int productId, string category)
    {
        _logger.LogInformation("Invalidating caches for product {ProductId} and category '{Category}'", productId, category);

        // Invalidate individual product cache
        _cache.Remove($"product_{productId}");

        // Invalidate all products cache
        _cache.Remove("all_products");

        // Invalidate category cache
        _cache.Remove($"category_{category.ToLowerInvariant()}");

        // Invalidate low stock caches (all thresholds)
        for (int threshold = 1; threshold <= 10; threshold++)
        {
            _cache.Remove($"low_stock_{threshold}");
        }

        _logger.LogInformation("Cache invalidation completed for product {ProductId}", productId);
    }

     public async Task<string?> GetCachedValueAsync(string key)
    {
        // Try to get from distributed cache
        var value = await _distributedCache.GetStringAsync(key);
        if (value != null)
            return value;

        // Not in cache: fetch from source, then cache
        value = "some data from DB";
        await _distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return value;
    }
}
