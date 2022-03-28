
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RdxFileTransfer.EventBus;
using RdxFileTransfer.EventBus.RabbitMq;

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
            .AddHangfire(x => x.UseMemoryStorage())
            .Configure<RabbitMqConfig>(context.Configuration.GetSection(nameof(RabbitMqConfig))))
        .Build();


while(true)
{

}