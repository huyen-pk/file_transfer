namespace RdxFileTransfer.EventBus.BusEvents
{
    public class RabbitMqEvent : IEvent<RabbitMqEvent>
    {
        public DateTime CreatedAt { get; init; }
        public string RoutingKey { get; init; }
        public RabbitMqEvent(string routingKey, DateTime createdAt)
        {
            ArgumentNullException.ThrowIfNull(routingKey);
            ArgumentNullException.ThrowIfNull(createdAt);

            RoutingKey = routingKey;
            CreatedAt = createdAt;
        }

        public RabbitMqEvent(string routingKey)
        {
            ArgumentNullException.ThrowIfNull(routingKey);

            RoutingKey = routingKey;
        }
    }
}
