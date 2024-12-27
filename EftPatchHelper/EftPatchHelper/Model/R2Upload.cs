using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;

namespace EftPatchHelper.Model;

public class R2Upload : IFileUpload
{
    public string DisplayName { get; set; }
    public string ServiceName { get; set; }
    public string HubEntryText { get; set; }
    public FileInfo UploadFileInfo { get; }
    public bool AddHubEntry { get; }

    private readonly R2Helper _r2;

    public R2Upload(FileInfo file, R2Helper r2)
    {
        _r2 = r2;
        UploadFileInfo = file;
        ServiceName = $"R2::{_r2.BucketName}";
        DisplayName = $"{ServiceName} Upload";
        HubEntryText = $"Download from {ServiceName}";
        AddHubEntry = false;
    }
    
    public string GetLink()
    {
        return $"{_r2.ConnectedDomain}/{UploadFileInfo.Name}";
    }

    public async Task<bool> UploadAsync(IProgress<double>? progress = null)
    {
        return await _r2.UploadToBucketAsync(UploadFileInfo, progress);
    }
}