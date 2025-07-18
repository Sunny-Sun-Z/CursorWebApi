using CursorWebApi.Application;
using CursorWebApi.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CursorWebApi.Domain.Exceptions;

namespace CursorWebApi.Infrastructure;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult(_products.AsEnumerable());

    public Task<Product?> GetByIdAsync(int id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Product product)
    {
        if (product == null)
            throw new ValidationException("Product cannot be null.");

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ValidationException("Product name is required.");

        product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        if (product == null)
            throw new ValidationException("Product cannot be null.");

        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing == null)
            throw new ProductNotFoundException(product.Id);
        // var existing = _products.FirstOrDefault(p => p.Id == product.Id)
        //     ?? throw new ProductNotFoundException(product.Id);
        existing.Name = product.Name;
        existing.Price = product.Price;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new ProductNotFoundException(id);

        _products.Remove(product);
        return Task.CompletedTask;
    }
}
