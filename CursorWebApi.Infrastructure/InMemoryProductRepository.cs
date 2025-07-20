using CursorWebApi.Application;
using CursorWebApi.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CursorWebApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CursorWebApi.Infrastructure;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private readonly ILogger<InMemoryProductRepository> _logger;

    public InMemoryProductRepository(ILogger<InMemoryProductRepository> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult(_products.AsEnumerable());

    public Task<Product?> GetByIdAsync(int id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Product product)
    {
        if (product == null)
            throw new ValidationException("Product cannot be null.");

        // Infrastructure layer only handles persistence, not business rules
        product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        _products.Add(product);
        _logger.LogInformation("Product added: {@Product}", product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        if (product == null)
            throw new ValidationException("Product cannot be null.");
        var existing = _products.FirstOrDefault(p => p.Id == product.Id)
            ?? throw new ProductNotFoundException(product.Id);
        existing.Name = product.Name;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.StockQuantity = product.StockQuantity;
        _logger.LogInformation("Product updated: {@Product}", product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id)
            ?? throw new ProductNotFoundException(id);
        _products.Remove(product);
        _logger.LogWarning("Product deleted: {@Product}", product);
        return Task.CompletedTask;
    }
}
