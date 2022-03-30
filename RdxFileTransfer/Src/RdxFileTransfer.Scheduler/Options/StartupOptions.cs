using CommandLine;
using RdxFileTransfer.EventBus;

namespace RdxFileTransfer.Scheduler.Options
{
    internal class StartupOptions
    {
        [Option('b', "bus", Required = false,
            HelpText = @$"Configure event bus: rabbit => rabbitmq, simple => inmemory dummy bus. 
                        If not set, default to {nameof(RabbitMqEventBus)}")]
        public string EventBus { get; set; }
    }
}
