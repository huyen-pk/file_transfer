using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.Enums;
using System.Text.Json;

namespace RdxFileTransfer.Scheduler.Workers
{
    public class ScanWorker : IWorker
    {
        private readonly IEventBus _eventBus;

        public event EventHandler<JobFinishedEventArg> JobFinished;
        public ScanWorker(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        public void Start(string sourceFolder, string destinationFolder)
        {
            ScanFolder(sourceFolder, destinationFolder);
        }

        private void ScanFolder(string sourceFolder, string destinationFolder)
        {
            var extensions = EnumerateExtensions(sourceFolder);
            foreach (var ext in extensions)
            {
                QueueFiles(sourceFolder, destinationFolder, ext);
            }
        }
        private void QueueFiles(string sourcePath, string destinationPath, string extension)
        {
            if (!Directory.Exists(sourcePath))
            {
                _eventBus.Publish<ErrorTransferEvent>(
                    new ErrorTransferEvent(routingKey: extension, createdAt: DateTime.Now)
                    {
                        Message = $"{sourcePath} does not exist."
                    });
                return;
            }
            var files = Directory.EnumerateFiles(sourcePath, $"*.{extension}", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFile = Path.Combine(destinationPath, fileName);
                var eventArgs = new JobFinishedEventArg();
                _eventBus.Publish<TransferEvent>(
                        new TransferEvent(routingKey: extension, createdAt: DateTime.Now)
                        {
                            Status = TransferStatus.Queued,
                            SourcePath = file,
                            DestinationPath = newFile
                        });
                if (JobFinished != null)
                    JobFinished(this, eventArgs);
            }
        }

        private List<string> EnumerateExtensions(string sourceFolder)
        {
            var files = Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories);
            return files.Select(f => Path.GetExtension(f)).ToList();
        }
    }
}
