using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CRM.Application.Common.Logging;

/// <summary>
/// Logging extension methods for structured logging with context enrichment.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs an operation with execution time measurement.
    /// </summary>
    public static IDisposable BeginOperationScope<T>(
        this ILogger<T> logger,
        string operationName,
        params (string Key, object? Value)[] properties)
    {
        var stopwatch = Stopwatch.StartNew();
        var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = operationName,
            ["OperationStartTime"] = DateTime.UtcNow
        }.Concat(properties.ToDictionary(p => p.Key, p => p.Value ?? "null")));

        logger.LogInformation("Starting operation: {Operation}", operationName);

        return new OperationScope(scope, logger, operationName, stopwatch);
    }

    /// <summary>
    /// Logs an entity operation with entity context.
    /// </summary>
    public static IDisposable BeginEntityOperationScope<T>(
        this ILogger<T> logger,
        string operation,
        string entityType,
        Guid entityId,
        params (string Key, object? Value)[] additionalProperties)
    {
        var properties = new List<(string, object?)>
        {
            ("EntityType", entityType),
            ("EntityId", entityId)
        };
        properties.AddRange(additionalProperties);

        return logger.BeginOperationScope($"{operation}_{entityType}", properties.ToArray());
    }

    private class OperationScope : IDisposable
    {
        private readonly IDisposable? _innerScope;
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public OperationScope(IDisposable? innerScope, ILogger logger, string operationName, Stopwatch stopwatch)
        {
            _innerScope = innerScope;
            _logger = logger;
            _operationName = operationName;
            _stopwatch = stopwatch;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.LogInformation(
                "Completed operation: {Operation} in {ElapsedMilliseconds}ms",
                _operationName,
                _stopwatch.ElapsedMilliseconds);
            _innerScope?.Dispose();
        }
    }
}

