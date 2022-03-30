using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.RabbitMq;

namespace RdxFileTransfer
{
    public class Runner
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<Runner> _logger;

        public Runner(IEventBus eventBus, ILogger<Runner> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public void CommandFileTransfer(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException();
            _eventBus.Publish<TransferEvent>(new TransferEvent(routingKey: Queues.FOLDER_QUEUE, createdAt: DateTime.Now)
            {
                SourcePath = sourceFolder,
                DestinationPath = destinationFolder,
            });
            _logger.LogInformation($"{sourceFolder} folder succesfully queued to transfer.\n\n");
        }
    }
}
