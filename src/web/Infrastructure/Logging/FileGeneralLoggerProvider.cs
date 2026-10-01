
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plandokument.Application.Options.Logging;

namespace Plandokument.Infrastructure.Logging;

public class FileGeneralLoggerProvider : ILoggerProvider
{
    private readonly ApplicationSpecificLoggingSettings _options;

    public FileGeneralLoggerProvider(IOptions<ApplicationSpecificLoggingSettings> options)
    {
        _options = options.Value;
    }

    // Public implementation so callers with a concrete FileGeneralLoggerProvider
    // can call CreateLogger(...) directly (used in Program.cs for early logging).
    public ILogger CreateLogger(string categoryName) => new FileGeneralLogger(categoryName, _options);

    // Public Dispose implementation. If the provider acquires resources in the
    // future they should be cleaned up here.
    public void Dispose()
    {
    }

    // Explicit interface implementations to preserve interface-only usage.
    // This lets callers that hold an ILoggerProvider reference use the provider
    // via the interface while concrete callers can use the public members.
    ILogger ILoggerProvider.CreateLogger(string categoryName) => CreateLogger(categoryName);

    void IDisposable.Dispose() => Dispose();
}
