using CursorWebApi.Domain.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursorWebApi.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;

        public RabbitMQPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public Task PublishAsync<T>(string queueName, T message) where T : class
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish("", queueName, null, body);

            return Task.CompletedTask;
        }
    }
}
