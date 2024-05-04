namespace EftPatchHelper.Model;

public class ReleaseInfo
{
    public string AkiVersion { get; set; }
    public string ClientVersion { get; set; }
    public List<ReleaseInfoMirror> Mirrors { get; set; }
}