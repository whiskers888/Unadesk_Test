using Shared.Extensions;
using BackgroundWorker.Data;
using BackgroundWorker.Infrastructure.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddDatabase<BackgroundWorkerDbContext>(context.Configuration, "WorkerDb");
    services.AddRabbitMqConnectionManager();
    services.AddFileStorage();
    services.AddWorkerServices();
});

var host = builder.Build();

await host.MigrateDatabaseAsync<BackgroundWorkerDbContext>();

await host.RunAsync();