using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Messaging;
using Shared.Services.FileStorage;

namespace Shared.Extensions;

public static class ServiceExtensions
{
    public static void AddRabbitMqConnectionManager(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMqConnectionManager>();
    }

    public static void AddFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorage, LocalFileStorage>();
    }

    public static void AddDatabase<TContext>(this IServiceCollection services, IConfiguration config,
        string connectionStringName)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(config.GetConnectionString(connectionStringName)));
    }
    
    public static async Task MigrateDatabaseAsync<TContext>(this IHost host) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync();
    }
}