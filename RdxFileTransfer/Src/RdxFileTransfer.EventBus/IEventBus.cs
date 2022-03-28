using RdxFileTransfer.EventBus.Events;

namespace RdxFileTransfer.EventBus
{
    public interface IEventBus
    {
        void Publish<T>(IEvent<T> @event);
    }
}
