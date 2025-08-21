//using Microsoft.Extensions.Hosting;
//using CursorWebApi.Domain.Messaging;
//using CursorWebApi.Domain;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;

//namespace CursorWebApi.Infrastructure.Messaging
//{
//    public class ProductConsumerWorker : BackgroundService
//    {
//        private readonly IMessageConsumer _consumer;

//        public ProductConsumerWorker(IMessageConsumer consumer)
//        {
//            _consumer = consumer;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            await _consumer.ConsumeAsync<Product>("products", async product =>
//            {
//                Console.WriteLine($"📬 Received product: {product.Name}");
//                // Handle the message, e.g., store in DB or trigger a workflow
//            });

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await Task.Delay(Timeout.Infinite, stoppingToken);
//            }
//        }
//    }
//}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CursorWebApi.Domain.Messaging;
using CursorWebApi.Domain;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CursorWebApi.Infrastructure.Messaging;

public class ProductConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductConsumerWorker> _logger;

    public ProductConsumerWorker(IServiceScopeFactory scopeFactory, ILogger<ProductConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;  // inject a IServiceScopeFactory
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 ProductConsumerWorker started");

        using var scope = _scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();  // resolve IServiceScopeFactory to IMessageConsumer

        await consumer.ConsumeAsync<Product>("products", async product =>
        {
            // consume here.
            _logger.LogInformation("📥 Received product: {Name}", product.Name);
            await Task.Delay(500); // Simulate processing
            _logger.LogInformation("✅ Processed product: {Name}", product.Name);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        _logger.LogInformation("🛑 ProductConsumerWorker shutting down");
    }
}

