using EftPatchHelper.EftInfo;

namespace EftPatchHelper.Model
{
    public class Options
    {
        public bool IgnoreExistingDirectories = true;
        public EftClient TargetClient = null;
        public EftClient SourceClient = null;
    }
}
