using CommandLine;
using RdxFileTransfer.EventBus;

namespace RdxFileTransfer.Configs
{
    internal class CommandLineOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source folder to transfer files from.")]
        public string SourceFolder { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination folder to transfer files to.")]
        public string DestinationFolder { get; set; }

        //[Option('o', "orchestrator", Required = false, 
        //    HelpText = $"Configure orchestrator: k => kubernetes, sw => dockerswarm. If not set, default to {nameof(SimpleManualOrchestrator)}")]
        public string Orchestrator { get; set; }

        [Option('b', "bus", Required = false,
            HelpText = $"Configure event bus: rabbit => rabbitmq, azure => azure bus. If not set, default to {nameof(RabbitMqEventBus)}")]
        public string EventBus { get; set; }
    }
}
