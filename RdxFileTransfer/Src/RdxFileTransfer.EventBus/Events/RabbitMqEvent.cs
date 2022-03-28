namespace RdxFileTransfer.EventBus.Events
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
    }
}
