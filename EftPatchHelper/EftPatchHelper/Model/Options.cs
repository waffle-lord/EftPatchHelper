using EftPatchHelper.EftInfo;

namespace EftPatchHelper.Model
{
    public class Options
    {
        /// <summary>
        /// A value that says if the user opted to ignore existing directories.
        /// </summary>
        public bool IgnoreExistingDirectories = false;

        /// <summary>
        /// The target client used to generate patches
        /// </summary>
        public EftClient TargetClient = null;

        /// <summary>
        /// The source client used to generate patches
        /// </summary>
        public EftClient SourceClient = null;

        /// <summary>
        /// The path to the patch folder containing the patches that were generated
        /// </summary>
        public string OutputPatchPath = null;
    }
}
