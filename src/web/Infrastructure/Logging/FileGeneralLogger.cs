using Plandokument.Application.Options.Logging;
using System.Collections.Concurrent;

namespace Plandokument.Infrastructure.Logging;

internal class FileGeneralLogger : ILogger, IDisposable
{
    private readonly string _categoryName;
    private readonly ApplicationSpecificLoggingSettings _options;
    private readonly Task _workerTask;
    private readonly BlockingCollection<(LogLevel level, string message, Exception? ex)> _queue = new();
    private readonly CancellationTokenSource _cts = new();

    public FileGeneralLogger(string categoryName, ApplicationSpecificLoggingSettings options)
    {
        _categoryName = categoryName;
        _options = options;

        // Startar bakgrundsarbetsuppgift
        _workerTask = Task.Run(ProcessQueueAsync);
    }

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    //public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
    public bool IsEnabled(LogLevel logLevel) => !_cts.IsCancellationRequested;

    void ILogger.Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
        _queue.Add((logLevel, msg, exception));
    }

    private async Task ProcessQueueAsync()
    {
        foreach (var (level, message, ex) in _queue.GetConsumingEnumerable(_cts.Token))
        {
            try
            {
                //TODO: Vid exception, lägga till att skriva en förenklad loggpost i GeneralLogFile utan stacktrace medan ErrorLogFile får den fullständiga posten. Detta för att säkerställa att det alltid finns en loggpost i GeneralLogFile. Som det är nu skrivs det till antingen eller.
                string path = ex != null
                    ? Path.Combine(_options.RootPath, _options.ErrorLogFile)
                    : Path.Combine(_options.RootPath, _options.GeneralLogFile);

                path = ApplyRotation(path);
                await WriteToFileAsync(path, message, ex);
            }
            catch
            {
                // Undertryck loggfel för att inte krascha applikationen
            }
        }
    }

    private string ApplyRotation(string path)
    {
        if (!_options.RotationOfFile.Enabled)
            return path;

        //TODO: Hantera om flera filer enligt MaxRetainedFiles
        if (_options.RotationOfFile.Mode == RotationOfFileModesOptions.Date)
        {
            string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");
            string ext = Path.GetExtension(path);
            string baseName = Path.GetFileNameWithoutExtension(path);
            string dir = Path.GetDirectoryName(path)!;
            return Path.Combine(dir, $"{baseName}_{dateSuffix}{ext}");
        }
        else if (_options.RotationOfFile.Mode == RotationOfFileModesOptions.Size)
        {
            //TODO: Hantera om MaxFileSizeInMB är 0, negativt, orimligt stort, decimalt
            long maxBytes = _options.RotationOfFile.MaxFileSizeInMB * 1024 * 1024;
            if (File.Exists(path) && new FileInfo(path).Length > maxBytes)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
                string ext = Path.GetExtension(path);
                string baseName = Path.GetFileNameWithoutExtension(path);
                string dir = Path.GetDirectoryName(path)!;
                string rotatedFile = Path.Combine(dir, $"{baseName}_{timestamp}{ext}");
                File.Move(path, rotatedFile, overwrite: true);
            }
        }

        return path;
    }

    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    private async Task WriteToFileAsync(string path, string message, Exception? ex)
    {
        await _fileLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string fullMessage = ex != null
                ? $"{message}{Environment.NewLine}{ex}"
                : message;
            await File.AppendAllTextAsync(path, fullMessage + Environment.NewLine);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    void IDisposable.Dispose()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        try { _workerTask.Wait(2000); } catch { }
    }
}