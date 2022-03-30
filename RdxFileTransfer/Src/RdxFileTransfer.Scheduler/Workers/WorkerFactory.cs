using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;

namespace RdxFileTransfer.Scheduler.Workers
{
    internal interface IWorkerFactory
    {
        ScanWorker CreateScanWorker();
        TransferWorker CreateTransferWorker();
    }

    internal class WorkerFactory: IWorkerFactory
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerFactory _loggerFactory;

        public WorkerFactory(IEventBus eventBus, ILoggerFactory loggerFactory)
        {
            _eventBus = eventBus;
            _loggerFactory = loggerFactory;
        }
        public ScanWorker CreateScanWorker()
        {
            return new ScanWorker(_eventBus, _loggerFactory.CreateLogger<ScanWorker>());
        }
        public TransferWorker CreateTransferWorker()
        {
            return new TransferWorker(_eventBus, _loggerFactory.CreateLogger<TransferWorker>());
        }
    }
}
