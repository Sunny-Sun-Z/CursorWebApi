using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorWebApi.Domain.Messaging
{
    public interface IMessageConsumer
    {
        Task ConsumeAsync<T>(string queueName, Func<T, Task> handler) where T : class;
    }
}
