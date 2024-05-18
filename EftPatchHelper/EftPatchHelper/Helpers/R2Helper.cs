using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Helpers;

public class R2Helper
{
    private readonly AmazonS3Client? _client;
    public string ConnectedDomain { get; private set; }
    public string BucketName { get; private set; }

    public R2Helper(Settings settings, Options options)
    {
        ConnectedDomain = settings.R2ConnectedDomainUrl;
        BucketName = settings.R2BucketName;

        if (settings.UsingR2())
        {
            var creds = new BasicAWSCredentials(settings.R2AccessKeyId, settings.R2SecretKeyId);
            _client = new AmazonS3Client(creds, new AmazonS3Config
            {
                ServiceURL = settings.R2ServiceUrl,
            });
        }
    }

    /// <summary>
    /// Deletes all content in the bucket
    /// </summary>
    /// <returns>True if all contents of the bucket were deleted, otherwise false</returns>
    public async Task<bool> ClearBucketAsync()
    {
        if (_client == null)
        {
            AnsiConsole.MarkupLine("[red]Client is unavailable[/]");
            return false;
        }
        
        AnsiConsole.MarkupLine($"[blue]Getting bucket contents: {BucketName}[/]");
        var listBucketResponse = await _client.ListObjectsAsync(BucketName);

        if (listBucketResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            AnsiConsole.MarkupLine("[red]failed to get bucket contents[/]");
            return false;
        }

        if (listBucketResponse.S3Objects.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]bucket is empty[/]");
            return true;
        }
        
        AnsiConsole.MarkupLine("[blue]Removing old content[/]");
        foreach (var s3Object in listBucketResponse.S3Objects)
        {
            var deleteResponse = await _client.DeleteObjectAsync(BucketName, s3Object.Key);

            if ((int)deleteResponse.HttpStatusCode < 200 || (int)deleteResponse.HttpStatusCode > 299)
            {
                AnsiConsole.MarkupLine($"[red]failed to delete {BucketName}::{s3Object.Key}[/]");
                return false;
            }
            
            AnsiConsole.MarkupLine($"[green]{BucketName}::{s3Object.Key} removed[/]");
        }

        return true;
    }

    /// <summary>
    /// Upload a file into the bucket
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="progress">A progress object to track upload progress</param>
    /// <returns>True if the file was uploaded successfully, otherwise false</returns>
    public async Task<bool> UploadToBucketAsync(FileInfo file, IProgress<double>? progress = null)
    {
        if (_client == null)
        {
            AnsiConsole.MarkupLine("[red]Client is unavailable[/]");
            return false;
        }

        file.Refresh();

        if (!file.Exists)
        {
            AnsiConsole.MarkupLine($"[red]File '{file.Name}' does not exist[/]");
            return false;
        }
        
        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            FilePath = file.FullName,
            DisablePayloadSigning = true,
        };

        if (progress != null)
        {
            request.StreamTransferProgress = (sender, progressArgs) =>
            {
                progress.Report(progressArgs.PercentDone);
            };
        }
        
        var uploadResponse = await _client.PutObjectAsync(request);

        if (uploadResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            AnsiConsole.MarkupLine("[red]failed to upload file[/]");
            return false;
        }

        return true;
    }
}