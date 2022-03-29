namespace RdxFileTransfer.EventBus.BusEvents
{
    public interface IEvent<T>
    {
        public DateTime CreatedAt { get; init; }
    }
}