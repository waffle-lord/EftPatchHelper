using EftPatchHelper.Interfaces;
using GoFileSharp;
using GoFileSharp.Model;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.GoFileData.Wrappers;


namespace EftPatchHelper.Model
{
    public class GoFileUpload : IFileUpload
    {
        private GoFile _goFile;
        public FileInfo UploadFileInfo { get; private set; }
        private DirectLink? _directLink = null;
        private GoFileFile _uploadedFile;
        private string _folderId;

        public string DisplayName { get; set; }
        public string ServiceName { get; set; }
        public string HubEntryText { get; set; }
        public bool AddHubEntry { get; }

        public GoFileUpload(FileInfo file, string apiToken, string folderId)
        {
            _goFile = new GoFile(new GoFileOptions
            {
                ApiToken = apiToken
            });
            
            _folderId = folderId;
            UploadFileInfo = file;
            ServiceName = "GoFile";
            DisplayName = $"{ServiceName} Upload: {UploadFileInfo.Name}";
            HubEntryText = $"Download from {ServiceName}";
            AddHubEntry = true;
        }

        public string GetLink()
        {
            return _directLink?.Link ?? "";
        }

        public async Task<bool> UploadAsync(IProgress<double>? progress = null)
        {
            var folder = await _goFile.GetFolderAsync(_folderId);

            if (folder == null)
            {
                return false;
            }

            var uploadedFile = await folder.UploadIntoAsync(UploadFileInfo, progress);

            if(uploadedFile == null) return false;

            _directLink = await uploadedFile.AddDirectLink();
            
            if(_directLink == null)
            {
                return false;
            }

            _uploadedFile = uploadedFile;

            return true;
        }
    }
}
