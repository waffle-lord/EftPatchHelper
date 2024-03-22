using EftPatchHelper.Interfaces;
using WinSCP;

namespace EftPatchHelper.Model;

public class SftpUpload : IFileUpload
{
    private readonly SftpUploadInfo _sftpInfo;
    private readonly SessionOptions _sessionOptions;
    public string DisplayName { get; set; }
    public string ServiceName { get; set; }
    public string HubEntryText { get; set; }
    public bool AddHubEntry { get; }

    public FileInfo UploadFileInfo { get; }

    public SftpUpload(FileInfo file, SftpUploadInfo sftpInfo)
    {
        UploadFileInfo = file;
        _sftpInfo = sftpInfo;
        
        _sessionOptions = new SessionOptions
        {
            Protocol = Protocol.Sftp,
            UserName = _sftpInfo.Username,
            Password = _sftpInfo.Password,
            HostName = _sftpInfo.Hostname,
            PortNumber = _sftpInfo.Port,
            SshHostKeyFingerprint = _sftpInfo.HostKey
        };
        
        ServiceName = _sftpInfo.Hostname;
        DisplayName = $"{ServiceName} Upload: {UploadFileInfo.Name}";
        HubEntryText = $"Download from {ServiceName}";
        AddHubEntry = false;
    }
    
    public string GetLink()
    {
        return $"{_sftpInfo.HttpPath}/${UploadFileInfo.Name}";
    }

    public Task<bool> UploadAsync(IProgress<double>? progress = null)
    {
        TransferOptions transferOptions = new TransferOptions
        {
            TransferMode = TransferMode.Binary,
        };
        
        using Session session = new Session();

        if (progress != null)
        {
            session.FileTransferProgress += (_, args) => progress.Report(Math.Floor(args.FileProgress * 100));
        }

        try
        {
            session.Open(_sessionOptions);

            session.PutFiles(UploadFileInfo.FullName, $"{_sftpInfo.UploadPath}/{UploadFileInfo.Name}", false, transferOptions).Check();
            
            return Task.FromResult(true);
        }
        catch
        {
            // ignored
        }

        return Task.FromResult(false);
    }
}