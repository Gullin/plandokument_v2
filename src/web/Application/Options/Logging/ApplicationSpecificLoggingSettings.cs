namespace Plandokument.Application.Options.Logging;

public class ApplicationSpecificLoggingSettings
{
    public string RootPath { get; set; } = "log";
    public string GeneralLogFile { get; set; } = "general.log";
    public string ErrorLogFile { get; set; } = "error.log";
    public RotationOfFileOptions RotationOfFile { get; set; } = new();
    public StatisticsRequestOptions StatisticsRequest { get; set; } = new();
}
