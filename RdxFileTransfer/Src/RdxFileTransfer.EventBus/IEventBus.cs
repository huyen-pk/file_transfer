using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus.BusEvents;

namespace RdxFileTransfer.EventBus
{
    public interface IEventBus
    {
        void Publish<T>(IEvent<T> @event);
        void Subscribe<T>(IEvent<T> @event, AsyncEventHandler<BasicDeliverEventArgs> onReceived);
        void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> onReceived);
        bool IsQueueHandled(string key);
    }
}
