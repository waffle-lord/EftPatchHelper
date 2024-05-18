namespace EftPatchHelper.Model;

public class PatchInfo
{
    public int SourceClientVersion { get; set; }
    public int TargetClientVersion { get; set; }
    public List<DownloadMirror> Mirrors { get; set; }
}