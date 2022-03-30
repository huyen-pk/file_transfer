using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RdxFileTransfer.Scheduler.Workers.Test
{
    public class ScanWorkerTests
    {
        private static string _currentDir = TestContext.CurrentContext.TestDirectory;
        private static string _sampleFolder = Path.Combine(_currentDir, "Samples", "SrcFolder");
        private Fixture _fixture = new Fixture();

        [TestCase]
        public void Scan_Should_Queue_All_Files_From_Source_Folder()
        {
            var eventBus = new Mock<IEventBus>();
            var logger = new Mock<ILogger<ScanWorker>>();
            var extensions = new List<string> { "no_ext", "docx", "pdf", "xls", "txt"};
            var files = Directory.EnumerateFiles(_sampleFolder);
            var destinationFolder = Path.Combine(_currentDir, "Samples", "DesFolder");
            var SUT = new ScanWorker(eventBus.Object, logger.Object);

            SUT.Start(_sampleFolder, destinationFolder, _ => { return extensions; });
            eventBus.Verify(e => e.Publish(It.IsAny<IEvent<BaseBusEvent>>()), Times.Exactly(files.Count()));
            
            Directory.Delete(destinationFolder);
        }
    }
}