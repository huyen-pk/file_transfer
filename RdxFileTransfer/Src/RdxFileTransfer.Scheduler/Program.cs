
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.RabbitMq;
using RdxFileTransfer.Scheduler;
using RdxFileTransfer.Scheduler.Workers;

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
            .AddScoped<TransferWorker>()
            .AddScoped<ScanWorker>()
            .AddScoped<Orchestrator>()
            .AddHangfire(x => x.UseMemoryStorage())
            .Configure<RabbitMqConfig>(context.Configuration.GetSection(nameof(RabbitMqConfig))))
        .Build();

Start(host.Services);

void Start(IServiceProvider provider)
{
    using IServiceScope serviceScope = provider.CreateScope();
    var orchestrator = serviceScope.ServiceProvider.GetRequiredService<Orchestrator>();
    orchestrator.Start();
}

while (true)
{

}