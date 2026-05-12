using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using Shared.Models;
using Unadesk_Test.Data;

namespace Unadesk_Test.Infrastructure.Messaging;

public class ApiOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<ApiOutboxProcessor> logger,
    RabbitMqConnectionManager connectionManager,
    IConfiguration config)
    : OutboxProcessorBase<ApiDbContext>(scopeFactory, logger, connectionManager,
        config["RabbitMQ:ServicePrefix"] ?? "pdf")   
{
    protected override DbSet<OutboxMessage> GetOutboxSet(ApiDbContext dbContext)
        => dbContext.OutboxMessages;
}