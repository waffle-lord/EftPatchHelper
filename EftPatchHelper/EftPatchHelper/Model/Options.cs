using EftPatchHelper.EftInfo;

namespace EftPatchHelper.Model
{
    public class Options
    {
        /// <summary>
        /// Whether or not the user opted to ignore existing directories.
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

        /// <summary>
        /// Whether or not the user opted to create a release on gitea
        /// </summary>
        public bool CreateRelease = false;

        /// <summary>
        /// Whether or not to upload the patcher to gofile.io
        /// </summary>
        public bool UploadToGoFile = false;

        /// <summary>
        /// Whether or not to upload the pather to mega.io
        /// </summary>
        public bool UploadToMega = false;

        /// <summary>
        /// List of mirrors to upload to Gitea
        /// </summary>
        public Dictionary<string, string> MirrorList = new Dictionary<string, string>();
    }
}
