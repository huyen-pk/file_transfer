using RdxFileTransfer.EventBus.Enums;

namespace RdxFileTransfer.EventBus.BusEvents
{
    public class ErrorTransferEvent : RabbitMqEvent, IEvent<ErrorTransferEvent>
    {
        public ErrorTransferEvent(string routingKey, DateTime createdAt) 
            : base(routingKey, createdAt)
        {
        }

        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string Message { get; set; }
        public TransferError Error { get; set; }
    }
}
