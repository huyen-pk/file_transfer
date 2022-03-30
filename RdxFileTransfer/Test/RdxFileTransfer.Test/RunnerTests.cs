using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using System.IO;

namespace RdxFileTransfer.Tests
{
    public class RunnerTests
    {
        private static string _currentDir = TestContext.CurrentContext.TestDirectory;
        private static string _sampleFolder = Path.Combine(_currentDir, "Samples", "SrcFolder");
        [Test]
        public void Transfer_Command_Should_Be_Queued_To_Message_Queue()
        {
            var eventbus = new Mock<IEventBus>();
            var logger = new Mock<ILogger<Runner>>();
            var SUT = new Runner(eventbus.Object, logger.Object);
            SUT.CommandFileTransfer(_sampleFolder, It.IsAny<string>());
            eventbus.Verify(e => e.Publish(It.IsAny<IEvent<BaseBusEvent>>()), Times.Exactly(1));
        }

        [Test]
        public void Non_Existent_Folder_Arg_Should_Throw()
        {
            var ghostFolder = Path.Combine(_currentDir, "Samples", "NonExist");
            var eventbus = new Mock<IEventBus>();
            var logger = new Mock<ILogger<Runner>>();
            var SUT = new Runner(eventbus.Object, logger.Object);
            Assert.Throws<DirectoryNotFoundException>(()=> SUT.CommandFileTransfer(ghostFolder, It.IsAny<string>()));
            eventbus.Verify(e => e.Publish(It.IsAny<IEvent<BaseBusEvent>>()), Times.Never);
        }
    }
}