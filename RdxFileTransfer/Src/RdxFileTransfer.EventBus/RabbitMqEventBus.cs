using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.RabbitMq;
using System.Text.Json;

namespace RdxFileTransfer.EventBus
{
    /// <summary>
    /// RabbitMq client.
    /// </summary>
    public class RabbitMqEventBus : IEventBus
    {
        private readonly HashSet<string> _queueHandlers;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IModel _channel;
        private readonly RabbitMqConfig _rabbitMqConfig;
        private readonly ILogger<RabbitMqEventBus> _logger;

        /// <inheritdoc>/>
        public RabbitMqEventBus(IOptions<RabbitMqConfig> config, ILogger<RabbitMqEventBus> logger)
        {
            _rabbitMqConfig = config.Value;
            _connectionFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(0.0),
                Uri = new Uri(_rabbitMqConfig.ServerUri)
            };
            var connection = _connectionFactory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _rabbitMqConfig.ExchangeKey,
                                        type: "direct");
            _queueHandlers = new HashSet<string>();
            _logger = logger;
            _logger.LogInformation($"Successfully configured and connected to RabbitMq server at {connection.Endpoint}");
        }

        /// <inheritdoc>/>
        public bool IsQueueHandled(string key)
        {
            return _queueHandlers.Contains(key);
        }

        /// <inheritdoc>/>
        public void Publish<T>(IEvent<T> @event)
        {
            var ev = @event as BaseBusEvent;
            var body = JsonSerializer.SerializeToUtf8Bytes(ev, typeof(T), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            if (ev != null)
            {
                _channel.QueueDeclare(
                    queue: ev.RoutingKey,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);
                _channel.QueueBind(
                    queue: ev.RoutingKey,
                    exchange: _rabbitMqConfig.ExchangeKey,
                    routingKey: ev.RoutingKey,
                    arguments: null);
                _channel.BasicPublish(exchange: _rabbitMqConfig.ExchangeKey,
                                    routingKey: ev.RoutingKey,
                                    basicProperties: null,
                                    body: body);
            }
        }

        /// <inheritdoc>/>
        public void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> onReceived)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += onReceived;
            _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);
            _channel.QueueBind(
                queue: queueName,
                exchange: _rabbitMqConfig.ExchangeKey,
                routingKey: queueName,
                arguments: null);
            _channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: consumer);
            if (!_queueHandlers.Contains(queueName))
                _queueHandlers.Add(queueName);
        }

        /// <inheritdoc>/>
        public void UnSubscribe(string queue, AsyncEventHandler<BasicDeliverEventArgs> onReceived)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received -= onReceived;
            _queueHandlers.Remove(queue);
        }

        /// <inheritdoc>/>
        public void Dispose()
        {
            _logger.LogInformation("Disposing event bus...");
            _channel.Dispose();
        }
    }
}
