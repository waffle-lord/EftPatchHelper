using EftPatchHelper.Interfaces;
using GoFileSharp;
using GoFileSharp.Model.GoFileData.Wrappers;

namespace EftPatchHelper.Model
{
    public class GoFileUpload : IFileUpload
    {
        private FileInfo _file;
        private GoFileFile _uploadedFile;

        public string DisplayName { get; set; }

        public GoFileUpload(FileInfo file, string apiToken)
        {
            GoFile.ApiToken = apiToken;
            _file = file;
            DisplayName = $"GoFile Upload: {_file.Name}";
        }

        public string GetLink()
        {
            return _uploadedFile.Link;
        }

        public async Task<bool> UploadAsync(IProgress<double>? progress = null)
        {
            var uploadedFile = await GoFile.UploadFileAsync(_file, progress);

            if(uploadedFile == null) return false;

            _uploadedFile = uploadedFile;

            return true;
        }
    }
}
