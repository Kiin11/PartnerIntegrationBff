using PartnerIntegrationBff.Interfact;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PartnerIntegrationBff.Services
{
    public class RabbitMqMessageProducer : IMessageProducer, IAsyncDisposable
    {
        private IConnection? _connection;
        private readonly IConnectionFactory _connectionFactory;
        private IChannel? _channel;
        private readonly SemaphoreSlim _semaphore = new (1,1);

        public RabbitMqMessageProducer(IConfiguration configuration)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            _semaphore.Dispose();
        }

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) return;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_channel is null)
                {
                    _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task PublishMessageAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
