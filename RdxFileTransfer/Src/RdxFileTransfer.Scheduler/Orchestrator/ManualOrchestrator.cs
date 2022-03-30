using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.RabbitMq;
using RdxFileTransfer.Scheduler.Workers;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RdxFileTransfer.Scheduler.Orchestrator
{
    /// <inheritdoc/>
    internal class ManualOrchestrator : IOrchestrator
    {
        private readonly IEventBus _eventBus;
        private readonly IWorkerFactory _workerFactory;
        private readonly ConcurrentDictionary<IWorker, EventHandler<JobFinishedEventArg>> _workers;
        public ManualOrchestrator(IEventBus eventBus, IWorkerFactory workerFactory)
        {
            _eventBus = eventBus;
            _workerFactory = workerFactory;
            _workers = new ConcurrentDictionary<IWorker, EventHandler<JobFinishedEventArg>>();
        }

        /// <inheritdoc/>
        public void Start()
        {
            _eventBus.Subscribe(Queues.FOLDER_QUEUE, onReceived: OnReceived);
        }

        private async Task OnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var body = @event.Body.ToArray();
            using (var stream = new MemoryStream(body))
            {
                var content = await JsonSerializer.DeserializeAsync<TransferEvent>(stream);
                if (content != null)
                    ScheduleJobs(content.SourcePath, content.DestinationPath);
            }
        }

        private void ScheduleJobs(string sourceFolder, string destinationFolder)
        {
            StartScanWorker(sourceFolder, destinationFolder);
            var extensions = EnumerateExtensions(sourceFolder);
            foreach (var ext in extensions)
            {
                if (!_eventBus.IsQueueHandled(ext))
                    StartTransferWorker(ext);
            }
        }

        private void StartScanWorker(string sourceFolder, string destinationFolder)
        {
            Task.Factory.StartNew(() =>
            {
                var worker = _workerFactory.CreateScanWorker();
                worker.JobFinished += OnScanFinished;
                _workers.TryAdd(worker, OnScanFinished);
                worker.Start(sourceFolder, destinationFolder, EnumerateExtensions);
            });
        }

        private void OnScanFinished(object? sender, JobFinishedEventArg e)
        {
        }

        private void StartTransferWorker(string extension)
        {
            Task.Factory.StartNew(() =>
            {
                var worker = _workerFactory.CreateTransferWorker();
                worker.JobFinished += OnTransferFinished;
                _workers.TryAdd(worker, OnTransferFinished);
                worker.Start(extension);
            });
        }
        private void OnTransferFinished(object? sender, JobFinishedEventArg e)
        {
        }

        private IEnumerable<string> EnumerateExtensions(string sourceFolder)
        {
            var files = Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories);
            return files
                    .Select(f => Path.GetExtension(f))
                    .Select(e => string.IsNullOrEmpty(e) ? Queues.EMPTY_EXTENSION_QUEUE : e)
                    .Distinct();
        }

        public void Dispose()
        {
            _eventBus.UnSubscribe(Queues.FOLDER_QUEUE, onReceived: OnReceived);
            foreach (var worker in _workers)
            {
                worker.Key.JobFinished -= worker.Value;
            }
        }
    }
}
