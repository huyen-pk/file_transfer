using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.Constants;
using RdxFileTransfer.EventBus.RabbitMq;
using RdxFileTransfer.Scheduler.Options;
using RdxFileTransfer.Scheduler.Orchestrator;
using RdxFileTransfer.Scheduler.Workers;

using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, configuration) =>
            configuration.AddEnvironmentVariables())
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
             services
             .AddScoped<IWorkerFactory, WorkerFactory>()
             .AddScoped<TransferWorker>()
             .AddScoped<ScanWorker>()
             .AddSingleton<IOrchestrator, ManualOrchestrator>();
         })
        .Build();


using IServiceScope serviceScope = host.Services.CreateScope();
using var orchestrator = serviceScope.ServiceProvider.GetRequiredService<IOrchestrator>();
orchestrator.Start();
Console.WriteLine("Job scheduler is running.");
Console.WriteLine("Type Esc to quit");
while (true)
{
    var input = Console.ReadKey();
    if (input.Key == ConsoleKey.Escape)
        break;
}
