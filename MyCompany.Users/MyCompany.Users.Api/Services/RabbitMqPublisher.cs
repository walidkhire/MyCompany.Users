using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Users.API.Services
{
    public class RabbitMqPublisher
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ConnectionFactory _factory;

        public RabbitMqPublisher()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public async Task InitializeAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: "users-created",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public async Task PublishUserCreated(string email)
        {
            if (_channel == null)
            {
                await InitializeAsync();
            }

            // Extraction dans une variable locale pour prouver au compilateur 
            // que la référence ne redeviendra pas nulle entre-temps
            var channel = _channel ?? throw new InvalidOperationException("Impossible d'initialiser le canal RabbitMQ.");

            var body = Encoding.UTF8.GetBytes(email);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "users-created",
                                            body: body);
        }
    }
}