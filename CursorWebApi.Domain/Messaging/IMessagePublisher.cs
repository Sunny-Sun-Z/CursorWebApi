using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorWebApi.Domain.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(string queueName, T message) where T : class;
    }
}
