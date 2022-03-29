namespace RdxFileTransfer.EventBus.BusEvents
{
    public class BaseBusEvent : IEvent<BaseBusEvent>
    {
        public DateTime CreatedAt { get; init; }
        public string RoutingKey { get; init; }
        public BaseBusEvent(string routingKey, DateTime createdAt)
        {
            ArgumentNullException.ThrowIfNull(routingKey);
            ArgumentNullException.ThrowIfNull(createdAt);

            RoutingKey = routingKey;
            CreatedAt = createdAt;
        }

        public BaseBusEvent(string routingKey)
        {
            ArgumentNullException.ThrowIfNull(routingKey);

            RoutingKey = routingKey;
        }
    }
}
