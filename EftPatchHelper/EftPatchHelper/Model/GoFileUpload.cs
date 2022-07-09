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
        public string ServiceName { get; set; }
        public string HubEntryText { get; set; }

        public GoFileUpload(FileInfo file, string apiToken)
        {
            GoFile.ApiToken = apiToken;
            _file = file;
            ServiceName = "GoFile";
            DisplayName = $"{ServiceName} Upload: {_file.Name}";
            HubEntryText = $"Download from {ServiceName}";
        }

        public string GetLink()
        {
            return _uploadedFile.DirectLink;
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
