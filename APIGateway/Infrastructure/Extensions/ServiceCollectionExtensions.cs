using Unadesk_Test.Infrastructure.Messaging;
using Unadesk_Test.Infrastructure.Messaging.Consumer;

namespace Unadesk_Test.Infrastructure.Extensions;

public static class ApiServiceExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "APIGateway.xml"));
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddHostedService<ApiOutboxProcessor>();
        services.AddHostedService<RabbitMqEventConsumer>();
    }
}