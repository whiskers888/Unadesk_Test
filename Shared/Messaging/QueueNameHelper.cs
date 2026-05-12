namespace Shared.Messaging;

public static class QueueNameHelper
{
    /// <summary>
    /// Генерирует имя очереди по соглашению: {prefix}.{eventNameLowercase}
    /// Префикс берётся из конфигурации (RabbitMQ:ServicePrefix)
    /// </summary>
    public static string GetQueueName(Type eventType, string servicePrefix)
    {
        var eventName = eventType.Name;
        var baseName = eventName.EndsWith("Event") ? eventName[..^5] : eventName;
        return $"{servicePrefix}.{baseName.ToLowerInvariant()}";
    }
}