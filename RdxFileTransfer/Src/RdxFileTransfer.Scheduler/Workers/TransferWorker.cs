using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.Enums;
using RdxFileTransfer.EventBus.RabbitMq;
using System.Text.Json;

namespace RdxFileTransfer.Scheduler.Workers
{
    /// <summary>
    /// Transfer files of from source folder to destination folder.
    /// </summary>
    internal class TransferWorker : IWorker
    {
        private IEventBus _eventBus;
        private readonly ILogger<TransferWorker> _logger;
        private string _queue;
        public TransferWorker(IEventBus eventBus, ILogger<TransferWorker> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public event EventHandler<JobFinishedEventArg> JobFinished;

        public void Start(string extension)
        {
            _eventBus.Subscribe(queueName: extension, onReceived: OnReceived);
            _queue = extension;
        }

        private async Task OnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var body = @event.Body.ToArray();
            using (var stream = new MemoryStream(body))
            {
                var content = await JsonSerializer.DeserializeAsync<TransferEvent>(stream);
                if (content != null)
                    StartTransfer(content.SourcePath, content.DestinationPath, _queue);
            }
        }
        private void StartTransfer(string sourcePath, string destinationPath, string extension)
        {
            if (!File.Exists(sourcePath))
            {
                _eventBus.Publish<ErrorTransferEvent>(
                    new ErrorTransferEvent(routingKey: extension, createdAt: DateTime.Now)
                    {
                        Message = $"{sourcePath} does not exist."
                    });
                return;
            }

            var eventArgs = new JobFinishedEventArg();
            try
            {
                File.Copy(sourcePath, destinationPath);
                _eventBus.Publish<TransferEvent>(
                                new TransferEvent(routingKey: Queues.SUCCESS_QUEUE, createdAt: DateTime.Now)
                                {
                                    Status = TransferStatus.Transferred,
                                    SourcePath = sourcePath,
                                    DestinationPath = destinationPath
                                });
                eventArgs.Status = TransferStatus.Transferred;
                _logger.LogInformation($"File transfered from {sourcePath} to {destinationPath}");
            }
            catch (FileNotFoundException ex)
            {
                var ev = new ErrorTransferEvent(routingKey: Queues.ERROR_QUEUE, createdAt: DateTime.Now)
                {
                    Error = TransferError.FileNotFound,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Message = ex.Message
                };
                eventArgs.Status = TransferStatus.Error;
                _eventBus.Publish<ErrorTransferEvent>(ev);
                _logger.LogTrace($"Error while transferring file from {sourcePath} to {destinationPath}", ex.StackTrace);
                _logger.LogError($"Error while transferring file from {sourcePath} to {destinationPath}", ex.Message);
            }
            catch (Exception ex)
            {
                var ev = new ErrorTransferEvent(routingKey: Queues.ERROR_QUEUE, createdAt: DateTime.Now)
                {
                    CreatedAt = DateTime.Now,
                    Error = TransferError.Unknown,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Message = ex.Message
                };
                eventArgs.Status = TransferStatus.Error;
                _eventBus.Publish<ErrorTransferEvent>(ev);
                _logger.LogTrace($"Error while transferring file from {sourcePath} to {destinationPath}", ex.StackTrace);
                _logger.LogError($"Error while transferring file from {sourcePath} to {destinationPath}", ex.Message);
            }
            finally
            {
                if (JobFinished != null)
                    JobFinished(this, eventArgs);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation($"Disposing worker...");
            _eventBus.UnSubscribe(_queue, OnReceived);
        }
    }
}
