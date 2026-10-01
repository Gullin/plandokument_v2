namespace Plandokument.Infrastructure.Process;

public static class ProcessExceptionHandling
{
    public static void Register(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Process");

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;

            logger.LogCritical(
                ex,
                "Unhandled process-level exception | IsTerminating={IsTerminating}",
                args.IsTerminating
            );
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            logger.LogError(
                args.Exception,
                "Unobserved Task exception"
            );

            args.SetObserved();
        };
    }
}
