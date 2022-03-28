namespace RdxFileTransfer.EventBus.Events
{
    public interface IEvent<T>
    {
        public DateTime CreatedAt { get; init; }
    }
}