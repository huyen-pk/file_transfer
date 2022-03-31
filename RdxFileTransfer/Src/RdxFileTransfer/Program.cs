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
               var eventBus = Environment.GetEnvironmentVariable(Env.EventBus) ?? Env.EventBus_Default;
               switch (eventBus)
               {
                   case Env.EventBus_RabbitMq:
                   default:
                       services
                          .AddSingleton<IEventBus, RabbitMqEventBus>()
                          .Configure<RabbitMqConfig>(context.Configuration.GetSection(nameof(RabbitMqConfig)));
                       break;
               }
               services.AddSingleton<Runner>();
           })
        .Build();

Start(host.Services, args);

static void Start(IServiceProvider provider, string[] args)
{
    using var serviceScope = provider.CreateScope();
    var runner = serviceScope.ServiceProvider.GetRequiredService<Runner>();
    Run(args, runner);

    while (true)
    {
        var input = Console.ReadLine();
        if (input == null)
            break;

        var arguments = input.Split(' ');

        Run(arguments, runner);
        Console.WriteLine("=====\nTo quit, use Ctrl + Z.\nTo transfer another folder, use command: -s [sourceFolder] -d [destinationFolder]\n====");
    }

}

static void Run(string[] args, Runner runner)
{
    CommandLineOptions options = null;
    Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed((cmdArgs) => options = cmdArgs);
    if (options != null)
    {
        try
        {
            runner.CommandFileTransfer(options.SourceFolder, options.DestinationFolder);
        }
        catch (DirectoryNotFoundException)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Folder must exist to be transferred.");
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}


