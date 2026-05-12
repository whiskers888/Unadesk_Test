using BackgroundWorker.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using Shared.Models;

namespace BackgroundWorker.Infrastructure.Messaging;

public class WorkerOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<WorkerOutboxProcessor> logger,
    RabbitMqConnectionManager connectionManager,
    IConfiguration config)
    : OutboxProcessorBase<BackgroundWorkerDbContext>(scopeFactory, logger, connectionManager,
        config["RabbitMQ:ServicePrefix"] ?? "pdf") 
{
    protected override DbSet<OutboxMessage> GetOutboxSet(BackgroundWorkerDbContext dbContext)
        => dbContext.OutboxMessages;
}