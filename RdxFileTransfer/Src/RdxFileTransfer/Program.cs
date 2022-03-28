using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RdxFileTransfer.Configs;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.Events;
using RdxFileTransfer.EventBus.RabbitMq;

CommandLineOptions _commandArgs = null;

using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureLogging((_, logging) =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options => options.IncludeScopes = true);
            logging.AddEventLog();
        })
        .ConfigureServices((context, services) =>
            services
            .AddSingleton<IEventBus, RabbitMqEventBus>()
            .Configure<RabbitMqConfig>(context.Configuration.GetSection(nameof(RabbitMqConfig))))
        .Build();



await Run(host.Services, args);

static async Task Run(IServiceProvider provider, string[] args)
{
    CommandLineOptions options = null;

    Parser.Default.ParseArguments<CommandLineOptions>(args)
        .WithParsed((cmdArgs) => options = cmdArgs);

    using IServiceScope serviceScope = provider.CreateScope();
    IEventBus eventBus = null;

    switch(options.EventBus)
    {
        default:
            var rabbitMqConfig = serviceScope.ServiceProvider.GetRequiredService<IOptions<RabbitMqConfig>>();
            eventBus = new RabbitMqEventBus(rabbitMqConfig.Value);
            break;
    }

    string? input;
    do
    {
        CommandFileTransfer(eventBus, options.SourceFolder, options.DestinationFolder);
        Console.WriteLine($"{options.SourceFolder} folder succesfully queued to transfer.\n");
        Console.WriteLine("=====\nTo quit, use Ctrl + Z.\nTo transfer another folder, use command: -s [sourceFolder] -d [destinationFolder]\n====");

        input = Console.ReadLine();
        if (input == null)
            break;

        var arguments = input.Split(' ');
        Parser.Default.ParseArguments<CommandLineOptions>(arguments)
                .WithParsed((cmdArgs) => options = cmdArgs);
    }
    while (true);
    
}

static void CommandFileTransfer(IEventBus eventBus, string sourceFolder, string destinationFolder)
{
    eventBus.Publish<TransferEvent>(new TransferEvent(routingKey: "folders", createdAt: DateTime.Now)
    {
        SourcePath = sourceFolder,
        DestinationPath = destinationFolder,
    });
}


