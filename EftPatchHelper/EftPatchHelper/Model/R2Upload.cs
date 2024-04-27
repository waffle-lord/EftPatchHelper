using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EftPatchHelper.Interfaces;
using Spectre.Console;

namespace EftPatchHelper.Model;

public class R2Upload : IFileUpload
{
    public string DisplayName { get; set; }
    public string ServiceName { get; set; }
    public string HubEntryText { get; set; }
    public FileInfo UploadFileInfo { get; }
    public bool AddHubEntry { get; }

    private readonly string _bucketName;
    private readonly string _connectedDomainUrl;
    private readonly IAmazonS3 _s3Client;

    public R2Upload(string connectedDomainUrl, string serviceUrl, string accessKey, string secretKey, string bucketName)
    {
        _bucketName = bucketName;
        _connectedDomainUrl = connectedDomainUrl;
        
        var creds = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(creds, new AmazonS3Config
        {
            ServiceURL = serviceUrl,
        });

        AddHubEntry = false;
    }
    
    public string GetLink()
    {
        return $"{_connectedDomainUrl}/{UploadFileInfo.Name}";
    }

    public async Task<bool> UploadAsync(IProgress<double>? progress = null)
    {
        var uploadRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            FilePath = UploadFileInfo.FullName,
            DisablePayloadSigning = true,
        };

        if (progress != null)
        {
            uploadRequest.StreamTransferProgress = (sender, progressArgs) =>
            {
                progress.Report(progressArgs.PercentDone);
            };
        }
        
        var uploadResponse = await _s3Client.PutObjectAsync(uploadRequest);

        return uploadResponse.HttpStatusCode == HttpStatusCode.OK;
    }
}