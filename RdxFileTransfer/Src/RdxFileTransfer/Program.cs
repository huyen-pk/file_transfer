using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RdxFileTransfer;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.BusEvents;
using RdxFileTransfer.EventBus.Constants;
using RdxFileTransfer.EventBus.RabbitMq;

using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureLogging((_, logging) =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options => options.IncludeScopes = true);
            logging.AddEventLog();
        })
        .ConfigureServices((context, services) =>
           {
               StartupOptions startupOptions = null;

               Parser.Default.ParseArguments<StartupOptions>(args)
                   .WithParsed((cmdArgs) => startupOptions = cmdArgs);
               if (startupOptions != null)
                   startupOptions = new StartupOptions()
                   {
                       EventBus = Environment.GetEnvironmentVariable(Env.EventBus) ?? Env.EventBus_Default
                   };
               switch (startupOptions?.EventBus)
               {
                   case Env.EventBus_RabbitMq:
                   default:
                       services
                          .AddSingleton<IEventBus, RabbitMqEventBus>()
                          .Configure<RabbitMqConfig>(context.Configuration.GetSection(nameof(RabbitMqConfig)));
                       break;
               }
           })
        .Build();

Run(host.Services, args);

static void Run(IServiceProvider provider, string[] args)
{
    Console.WriteLine("=====\nTo quit, use Ctrl + Z.\nTo transfer another folder, use command: -s [sourceFolder] -d [destinationFolder]\n====");
    using var serviceScope = provider.CreateScope();
    var eventBus = serviceScope.ServiceProvider.GetRequiredService<IEventBus>();
    while (true)
    {
        var input = Console.ReadLine();
        if (input == null)
            break;

        CommandLineOptions options = null;
        var arguments = input.Split(' ');
        Parser.Default.ParseArguments<CommandLineOptions>(arguments)
                .WithParsed((cmdArgs) => options = cmdArgs);
        if (options == null)
            continue;

        CommandFileTransfer(eventBus,
                            options.SourceFolder,
                            options.DestinationFolder);

        Console.WriteLine("=====\nTo quit, use Ctrl + Z.\nTo transfer another folder, use command: -s [sourceFolder] -d [destinationFolder]\n====");
    }

}

static void CommandFileTransfer(IEventBus eventBus, string sourceFolder, string destinationFolder)
{
    eventBus.Publish<TransferEvent>(new TransferEvent(routingKey: Queues.FOLDER_QUEUE, createdAt: DateTime.Now)
    {
        SourcePath = sourceFolder,
        DestinationPath = destinationFolder,
    });
    Console.WriteLine($"{sourceFolder} folder succesfully queued to transfer.\n\n");
}


