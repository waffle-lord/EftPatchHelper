using EftPatchHelper.EftInfo;

namespace EftPatchHelper.Model
{
    public class Options
    {
        public bool IgnoreExistingDirectories = false;
        public EftClient TargetClient = null;
        public EftClient SourceClient = null;
        public string OutputPatchPath = null;
    }
}
