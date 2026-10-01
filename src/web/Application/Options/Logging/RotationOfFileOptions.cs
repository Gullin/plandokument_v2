namespace Plandokument.Application.Options.Logging;

public class RotationOfFileOptions
{
    public bool Enabled { get; set; } = false;
    public RotationOfFileModesOptions Mode { get; set; } = RotationOfFileModesOptions.Size;
    public long MaxFileSizeInMB { get; set; } = 10;
    public int MaxRetainedFiles { get; set; } = 10;
}
