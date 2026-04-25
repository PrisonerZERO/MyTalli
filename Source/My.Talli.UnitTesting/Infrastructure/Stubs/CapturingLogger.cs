namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Microsoft.Extensions.Logging;

/// <summary>Logger</summary>
public class CapturingLogger<T> : ILogger<T>
{
    #region <Properties>

    public List<(LogLevel Level, string Message)> Entries { get; } = new();

    #endregion

    #region <Methods>

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add((logLevel, formatter(state, exception)));
    }

    #endregion

    #region <Nested>

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }

    #endregion
}
