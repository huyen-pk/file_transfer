using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus.Events;
using RdxFileTransfer.EventBus.RabbitMq;
using System.Text.Json;

namespace RdxFileTransfer.EventBus
{
    public class RabbitMqEventBus : IEventBus
    {
        private ConnectionFactory _connectionFactory;
        private IModel _channel;
        private RabbitMqConfig _rabbitMqConfig;
        public RabbitMqEventBus(RabbitMqConfig config)
        {
            _rabbitMqConfig = config;
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

            _channel.BasicConsume(
                queue: ev.RoutingKey,
                autoAck: false,
                consumer: consumer);
        }
    }
}
