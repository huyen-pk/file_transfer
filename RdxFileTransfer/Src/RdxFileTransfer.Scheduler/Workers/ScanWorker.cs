using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.Enums;
using RdxFileTransfer.EventBus.RabbitMq;

namespace RdxFileTransfer.Scheduler.Workers
{
    /// <summary>
    /// Scans to-be-transfered folder and queue files according to their extensions.
    /// </summary>
    public class ScanWorker : IWorker
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<ScanWorker> _logger;

        public event EventHandler<JobFinishedEventArg> JobFinished;
        public ScanWorker(IEventBus eventBus, ILogger<ScanWorker> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }
        public void Start(
            string sourceFolder,
            string destinationFolder,
            Func<string, IEnumerable<string>> enumerateExtension)
        {
            ScanFolder(sourceFolder, destinationFolder, enumerateExtension);
        }

        public void ScanFolder(
            string sourceFolder,
            string destinationFolder,
            Func<string, IEnumerable<string>> enumerateExtensions)
        {
            if (!Directory.Exists(sourceFolder))
            {
                _logger.LogError($"Source directory {sourceFolder} does not exist. Quit processing command.");
                _eventBus.Publish<ErrorTransferEvent>(
                    new ErrorTransferEvent(routingKey: Queues.ERROR_QUEUE, createdAt: DateTime.Now)
                    {
                        Message = $"{sourceFolder} does not exist."
                    });
                return;
            }
            if (!Directory.Exists(destinationFolder))
            {
                _logger.LogInformation($"Destination directory {sourceFolder} does not exist. Create directory...");
                Directory.CreateDirectory(destinationFolder);
            }

            var extensions = enumerateExtensions(sourceFolder);
            foreach (var ext in extensions)
            {
                _logger.LogInformation($"Queuing all files of type {ext} from {sourceFolder}.");
                QueueFiles(sourceFolder, destinationFolder, ext);
            }
        }
        private void QueueFiles(string sourcePath, string destinationPath, string extension)
        {
            var files = EnumerateFiles(sourcePath, extension);

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
                InvokeOnFinished(eventArgs);
                _logger.LogInformation($"File queued : {file}");
            }
        }
        private IEnumerable<string> EnumerateFiles(string folderPath, string extension)
        {
            if (extension == Queues.EMPTY_EXTENSION_QUEUE)
            {
                return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)));
            }
            else
            {
                return Directory.EnumerateFiles(folderPath, $"*{extension}", SearchOption.AllDirectories);
            }
        }
        private void InvokeOnFinished(JobFinishedEventArg eventArgs)
        {
            if (JobFinished != null)
                JobFinished(this, eventArgs);
        }
        public void Dispose()
        {
            _logger.LogInformation($"Disposing worker...");
            if (JobFinished != null)
            {
                var handlers = JobFinished.GetInvocationList();
                foreach (var handler in handlers)
                    JobFinished -= handler as EventHandler<JobFinishedEventArg>;
            }
        }
    }
}
