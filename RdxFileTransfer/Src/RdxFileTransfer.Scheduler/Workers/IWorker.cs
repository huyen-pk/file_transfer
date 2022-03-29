using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.Enums;

namespace RdxFileTransfer.Scheduler.Workers
{
    public interface IWorker
    {
        event EventHandler<JobFinishedEventArg> JobFinished;
    }

    public class JobFinishedEventArg: EventArgs
    {
        public TransferStatus Status { get; set; }
        public string Message { get; set; }
    }
}
