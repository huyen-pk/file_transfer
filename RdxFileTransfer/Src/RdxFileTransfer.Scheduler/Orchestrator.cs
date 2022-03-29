using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.Scheduler.Workers;
using System.Text.Json;

namespace RdxFileTransfer.Scheduler
{
    internal class Orchestrator
    {
        private readonly IEventBus _eventBus;

        public Orchestrator(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Start()
        {
            _eventBus.Subscribe("transfer_jobs", onReceived: OnReceived);
        }

        private async Task OnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var body = @event.Body.ToArray();
            using (var stream = new MemoryStream(body))
            {
                var content = await JsonSerializer.DeserializeAsync<TransferEvent>(stream);
                ScheduleJobs(content.SourcePath, content.DestinationPath);
            }
        }

        public void ScheduleJobs(string sourceFolder, string destinationFolder)
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
                var worker = new ScanWorker(_eventBus);
                worker.JobFinished += OnScanFinished;
                worker.Start(sourceFolder, destinationFolder);
            });
        }

        private void OnScanFinished(object? sender, JobFinishedEventArg e)
        {
        }

        private void StartTransferWorker(string extension)
        {
            Task.Factory.StartNew(() =>
            {
                var worker = new TransferWorker(_eventBus);
                worker.JobFinished += OnTransferFinished;
                worker.Start(extension);
            });
        }
        private void OnTransferFinished(object? sender, JobFinishedEventArg e)
        {
        }

        private List<string> EnumerateExtensions(string sourceFolder)
        {
            var files = Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories);
            return files.Select(f => Path.GetExtension(f)).ToList();
        }
    }
}
