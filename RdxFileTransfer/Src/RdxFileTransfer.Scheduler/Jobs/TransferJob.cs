using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.Events;

namespace RdxFileTransfer.Scheduler.Jobs
{
    internal class TransferJob : ITransferJob
    {
        private IEventBus _eventBus;
        public TransferJob(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        public void Transfer(string sourceFolder, string destinationFolder, string extension)
        {
            if (!Directory.Exists(sourceFolder))
            {
                _eventBus.Publish<ErrorTransferEvent>(
                    new ErrorTransferEvent(routingKey: extension, createdAt: DateTime.Now)
                {
                    Message = $"{sourceFolder} does not exist."
                });
                return;
            }
            var files = Directory.EnumerateFiles(sourceFolder, $"*.{extension}", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFile = Path.Combine(destinationFolder, fileName);
                try
                {
                    
                    File.Copy(file, newFile);
                    _eventBus.Publish<TransferEvent>(
                        new TransferEvent(routingKey: extension, createdAt: DateTime.Now)
                        {
                            Status = TransferStatus.Transferred,
                            SourcePath = file,
                            DestinationPath = newFile
                        });
                }
                catch(FileNotFoundException ex)
                {
                    var ev = new ErrorTransferEvent(routingKey: extension, createdAt: DateTime.Now)
                    {
                        Error = TransferError.FileNotFound,
                        SourcePath = file,
                        DestinationPath = newFile,
                        Message = ex.Message
                    };

                    _eventBus.Publish<ErrorTransferEvent>(ev);
                }
                catch (Exception ex)
                {
                    var ev = new ErrorTransferEvent(routingKey: extension, createdAt: DateTime.Now)
                    {
                        CreatedAt = DateTime.Now,
                        Error = TransferError.Unknown,
                        SourcePath = file,
                        DestinationPath = newFile,
                        Message = ex.Message
                    };

                    _eventBus.Publish<ErrorTransferEvent>(ev);
                }
            }
        }
    }
}
