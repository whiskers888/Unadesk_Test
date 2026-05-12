using BackgroundWorker.Infrastructure.Messaging;
using BackgroundWorker.Infrastructure.Messaging.Consumer;
using BackgroundWorker.Services.TextExtractor;

namespace BackgroundWorker.Infrastructure.Extensions;

public static class WorkerServiceExtensions
{
    public static void AddWorkerServices(this IServiceCollection services)
    {
        services.AddHostedService<RabbitMqConsumer>();
        services.AddHostedService<WorkerOutboxProcessor>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddScoped<ITextExtractor, PdfTextExtractor>();
    }
}