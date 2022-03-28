namespace RdxFileTransfer.EventBus.Events
{
    public enum TransferStatus
    {
        Queued = 1,
        Transferred = 2, 
        Error = 3,
    }
}