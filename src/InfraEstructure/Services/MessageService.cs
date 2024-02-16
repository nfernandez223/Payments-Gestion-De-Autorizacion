using Application.Interfaces;
using Domain.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraEstructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueNameConsume;
        private readonly string _queueNamePublish;

        public MessageService(string hostName, string queueNameConsume, string queueNamePublish)
        {
            _factory = new ConnectionFactory() { HostName = hostName };
            _queueNameConsume = queueNameConsume;
            _queueNamePublish = queueNamePublish;
        }

        public void SendMessage(Solicitud solicitud)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueNamePublish, durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message = JsonConvert.SerializeObject(solicitud);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: _queueNamePublish, basicProperties: null, body: body);
            }
        }

        public async Task<Solicitud> ConsumeAsync()
        {
            Solicitud? solicitud = null;

            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueNameConsume,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                var tcs = new TaskCompletionSource<bool>();

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    solicitud = JsonConvert.DeserializeObject<Solicitud>(message);
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(true);
                    }
                };

                channel.BasicConsume(queue: _queueNameConsume,
                                     autoAck: true,
                                     consumer: consumer);
                await tcs.Task;

                return solicitud;
            }
        }
    }
}
