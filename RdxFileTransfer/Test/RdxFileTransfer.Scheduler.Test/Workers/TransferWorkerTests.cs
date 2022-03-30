using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.Scheduler.Workers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RdxFileTransfer.Scheduler.Tests.Workers
{
    public class TransferWorkerTests
    {
        private static string _currentDir = TestContext.CurrentContext.TestDirectory;
        private static string _sampleFolder = Path.Combine(_currentDir, "Samples", "SrcFolder");
        private Fixture _fixture = new Fixture();

        [TestCase]
        public void Non_Existent_File_Should_Be_Queued_As_Error()
        {
            var sourceFile = Path.Combine(_currentDir, "Samples", "NonExist");
            var eventBus = new Mock<IEventBus>();
            var logger = new Mock<ILogger<TransferWorker>>();
            var extensions = new List<string> { "no_ext", "docx", "pdf", "xls", "txt" };
            var files = Directory.EnumerateFiles(_sampleFolder);
            
            var destinationFolder = Path.Combine(_currentDir, "Samples", "DesFolder");
            Directory.CreateDirectory(destinationFolder);

            var destinationFile = Path.Combine(destinationFolder, "NonExist");
            var SUT = new TransferWorker(eventBus.Object, logger.Object);

            SUT.StartTransfer(sourceFile, destinationFile, _fixture.Create<string>());
            eventBus.Verify(e => e.Publish(It.IsAny<IEvent<ErrorTransferEvent>>()), Times.Once);
            eventBus.Verify(e => e.Publish(It.IsAny<IEvent<TransferEvent>>()), Times.Never);
        }

        [TestCase]
        public void Should_Copy_File_Successfully_If_Exists()
        {
            var sourceFile = Directory.EnumerateFiles(_sampleFolder).First();
            var eventBus = new Mock<IEventBus>();
            var logger = new Mock<ILogger<TransferWorker>>();
            var extensions = new List<string> { "no_ext"};
            var files = Directory.EnumerateFiles(_sampleFolder);

            var destinationFolder = Path.Combine(_currentDir, "Samples", "DesFolder");
            Directory.CreateDirectory(destinationFolder);

            var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
            var SUT = new TransferWorker(eventBus.Object, logger.Object);

            SUT.StartTransfer(sourceFile, destinationFile, _fixture.Create<string>());
            eventBus.Verify(e => e.Publish(It.IsAny<IEvent<TransferEvent>>()), Times.Once);
            File.Exists(destinationFile);
            File.Delete(destinationFile);
        }
    }
}
