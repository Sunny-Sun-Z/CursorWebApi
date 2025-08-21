//using MyApp.Domain.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using CursorWebApi.Domain.Messaging;

namespace MyApp.Infrastructure.Messaging;

public class RabbitMQConsumer : IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(IConnection connection, ILogger<RabbitMQConsumer> logger)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _logger = logger;
    }

    public Task ConsumeAsync<T>(string queueName, Func<T, Task> handler) where T : class
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            
            //var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray()));
            var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.Span));

            if (message != null)
            {
                await handler(message);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            else
            {
                _logger.LogWarning("Received null message.");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
