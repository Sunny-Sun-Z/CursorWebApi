using CursorWebApi.Application;
using CursorWebApi.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

  namespace CursorWebApi.Infrastructure;

  public class InMemoryProductRepository : IProductRepository
  {
      private readonly List<Product> _products = new();

      public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult(_products.AsEnumerable());

      public Task<Product?> GetByIdAsync(int id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

      public Task AddAsync(Product product)
      {
          product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
          _products.Add(product);
          return Task.CompletedTask;
      }

      public Task UpdateAsync(Product product)
      {
          var existing = _products.FirstOrDefault(p => p.Id == product.Id);
          if (existing != null)
          {
              existing.Name = product.Name;
              existing.Price = product.Price;
          }
          return Task.CompletedTask;
      }

      public Task DeleteAsync(int id)
      {
          var product = _products.FirstOrDefault(p => p.Id == id);
          if (product != null)
              _products.Remove(product);
          return Task.CompletedTask;
      }
  }
