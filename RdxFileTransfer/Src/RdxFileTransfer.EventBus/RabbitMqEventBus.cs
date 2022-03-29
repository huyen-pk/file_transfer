using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.RabbitMq;
using System.Text.Json;

namespace RdxFileTransfer.EventBus
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly HashSet<string> _queueHandlers;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IModel _channel;
        private readonly RabbitMqConfig _rabbitMqConfig;
        public RabbitMqEventBus(IOptions<RabbitMqConfig> config)
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
        }

        public bool IsQueueHandled(string key)
        {
            return _queueHandlers.Contains(key);
        }

        public void Publish<T>(IEvent<T> @event)
        {
            var ev = @event as RabbitMqEvent;
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

        public void Subscribe<T>(IEvent<T> @event, AsyncEventHandler<BasicDeliverEventArgs> onReceived)
        {
            var ev = @event as RabbitMqEvent;
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += onReceived;
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
            _channel.BasicConsume(
                queue: ev.RoutingKey,
                autoAck: false,
                consumer: consumer);
            if (!_queueHandlers.Contains(ev.RoutingKey))
                _queueHandlers.Add(ev.RoutingKey);
        }

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

        public void UnSubscribe<T>(IEvent<T> @event, AsyncEventHandler<BasicDeliverEventArgs> onReceived)
        {
            var ev = @event as RabbitMqEvent;
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received -= onReceived;

            _channel.BasicConsume(
                queue: ev.RoutingKey,
                autoAck: false,
                consumer: consumer);
            _queueHandlers.Remove(ev.RoutingKey);
        }
    }
}
