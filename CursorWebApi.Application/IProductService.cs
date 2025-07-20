using CursorWebApi.Domain;

namespace CursorWebApi.Application;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(string name, decimal price, string category, int stockQuantity = 0);
    Task UpdateProductAsync(int id, string name, decimal price, string category);
    Task DeleteProductAsync(int id);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 5);
    Task<decimal> CalculateProductDiscountAsync(int productId, decimal discountPercentage);
}
