using RabbitMQ.Client;
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

        // Note : En .NET moderne, on préfère initialiser la connexion de manière asynchrone
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
            if (_channel == null) await InitializeAsync();

            var body = Encoding.UTF8.GetBytes(email);

            // BasicPublish est devenu BasicPublishAsync
            // L'échange vide "" est l'échange par défaut
            await _channel.BasicPublishAsync(exchange: "",
                                            routingKey: "users-created",
                                            body: body);
        }
    }
}