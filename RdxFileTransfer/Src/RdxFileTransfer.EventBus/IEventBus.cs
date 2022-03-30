using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus.BusEvents;

namespace RdxFileTransfer.EventBus
{
    public interface IEventBus: IDisposable
    {
        /// <summary>
        /// Publish an event.
        /// </summary>
        /// <typeparam name="T">Type of the event.</typeparam>
        /// <param name="event">Event.</param>
        void Publish<T>(IEvent<T> @event);

        /// <summary>
        /// Subscribe to an event.
        /// </summary>
        /// <param name="queueName">Message queue.</param>
        /// <param name="onReceived">Handle message from queue.</param>
        void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> onReceived);

        /// <summary>
        /// Check if a queue is already handled by a worker process.
        /// </summary>
        /// <param name="key">Message queue.</param>
        /// <returns>True if worker process for the queue exists.</returns>
        bool IsQueueHandled(string key);

        /// <summary>
        /// Unsubscribe from a queue. Stop listening to message from this queue.
        /// </summary>
        /// <param name="queue">Message queue.</param>
        /// <param name="onReceived">Handler to unsubscribe.</param>
        void UnSubscribe(string queue, AsyncEventHandler<BasicDeliverEventArgs> onReceived);
    }
}
