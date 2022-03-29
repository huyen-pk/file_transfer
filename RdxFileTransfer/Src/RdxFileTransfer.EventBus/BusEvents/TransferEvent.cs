using RdxFileTransfer.EventBus.Enums;

namespace RdxFileTransfer.EventBus.BusEvents
{
    public class TransferEvent: BaseBusEvent, IEvent<TransferEvent>
    {
        public TransferEvent(string routingKey, DateTime createdAt) 
            : base(routingKey, createdAt)
        {
        }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string Message { get; set; }
        public TransferStatus Status { get; set; }
    }
}
