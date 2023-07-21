using EftPatchHelper.Interfaces;
using GoFileSharp;
using GoFileSharp.Model.GoFileData.Wrappers;

namespace EftPatchHelper.Model
{
    public class GoFileUpload : IFileUpload
    {
        public FileInfo UploadFileInfo { get; private set; }
        private GoFileFile _uploadedFile;
        private string _folderId;

        public string DisplayName { get; set; }
        public string ServiceName { get; set; }
        public string HubEntryText { get; set; }

        public GoFileUpload(FileInfo file, string apiToken, string folderId)
        {
            GoFile.ApiToken = apiToken;
            _folderId = folderId;
            UploadFileInfo = file;
            ServiceName = "GoFile";
            DisplayName = $"{ServiceName} Upload: {UploadFileInfo.Name}";
            HubEntryText = $"Download from {ServiceName}";
        }

        public string GetLink()
        {
            return _uploadedFile.DirectLink;
        }

        public async Task<bool> UploadAsync(IProgress<double>? progress = null)
        {
            var folder = await GoFile.GetFolder(_folderId);

            if (folder == null)
            {
                return false;
            }

            var uploadedFile = await folder.UploadIntoAsync(UploadFileInfo, progress);

            if(uploadedFile == null) return false;

            if(!await uploadedFile.SetDirectLink(true))
            {
                return false;
            }

            _uploadedFile = uploadedFile;

            return true;
        }
    }
}
