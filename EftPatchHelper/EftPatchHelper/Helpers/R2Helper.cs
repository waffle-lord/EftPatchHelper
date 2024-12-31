using System.Net;
using System.Security.Cryptography;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EftPatchHelper.Extensions;
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

    public async Task<bool> AbortMultipartUpload(string key, string id)
    {
        if (_client == null)
        {
            AnsiConsole.MarkupLine("[red]Client is unavailable[/]");
            return false;
        }
            
        var abortUploadRequest = new AbortMultipartUploadRequest()
        {
            BucketName = BucketName,
            Key = key,
            UploadId = id,
        };
            
        var abortResponse = await _client.AbortMultipartUploadAsync(abortUploadRequest);

        if ((int)abortResponse.HttpStatusCode <= 200 || (int)abortResponse.HttpStatusCode >= 300)
        {
            AnsiConsole.MarkupLine($"[red]  -> {Markup.Escape(key)} failed to abort[/]");
            return false;
        }
            
        AnsiConsole.MarkupLine($"[green]  -> {Markup.Escape(key)} aborted[/]");
        return true;
    }

    public async Task<List<MultipartUpload>> ListMultiPartUploads()
    {
        if (_client == null)
        {
            AnsiConsole.MarkupLine("[red]Client is unavailable[/]");
            return [];
        }

        var listRequest = new ListMultipartUploadsRequest()
        {
            BucketName = BucketName,
        };
        
        var response = await _client.ListMultipartUploadsAsync(listRequest);

        return response.MultipartUploads;
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
            AnsiConsole.MarkupLine("[red]Failed to get bucket contents[/]");
            return false;
        }

        if (listBucketResponse.S3Objects.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Bucket is empty[/]");
            return true;
        }
        
        AnsiConsole.MarkupLine("[blue]Removing old content[/]");
        foreach (var s3Object in listBucketResponse.S3Objects)
        {
            var deleteResponse = await _client.DeleteObjectAsync(BucketName, s3Object.Key);

            if ((int)deleteResponse.HttpStatusCode < 200 || (int)deleteResponse.HttpStatusCode > 299)
            {
                AnsiConsole.MarkupLine($"[red]Failed to delete {BucketName}::{s3Object.Key}[/]");
                return false;
            }
            
            AnsiConsole.MarkupLine($"[green]{BucketName}::{s3Object.Key} removed[/]");
        }

        return true;
    }

    private async Task<bool> PutRequestUpload(FileInfo file, IProgress<double>? progress)
    {
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

        if (uploadResponse.HttpStatusCode.IsSuccessStatus())
        {
            return true;
        }
        
        AnsiConsole.MarkupLine("[red]Failed to upload file[/]");
        return false;

    }

    private async Task<bool> MultiPartUpload(FileInfo file, IProgress<double>? progress)
    {
        if (_client == null)
        {
            AnsiConsole.MarkupLine("[red]Client is unavailable[/]");
            return false;
        }

        var initiateRequest = new InitiateMultipartUploadRequest()
        {
            BucketName = BucketName,
            Key = file.Name,
        };
        
        AnsiConsole.MarkupLine("[blue]Initiating multipart upload[/]");
        
        List<UploadPartResponse> uploadedParts = new List<UploadPartResponse>();

        var initiateResponse = await _client.InitiateMultipartUploadAsync(initiateRequest);
        
        try
        {
            var mbSize = 100;
            var partSize = mbSize * (long)Math.Pow(2, 20); // mbSize in MB
            
            var estimatedPartsCount = (int)Math.Ceiling((double)file.Length / partSize);
            
            AnsiConsole.MarkupLine($"[blue]Esitmated Parts needed: {estimatedPartsCount} @ {mbSize}mb[/]");

            long filePosition = 0;
            for (int i = 1; filePosition < file.Length; i++)
            {
                AnsiConsole.MarkupLine($"[blue]Uploading Part[/]  [yellow]{i}[/] [blue]/ {estimatedPartsCount}[/]");

                var position = filePosition;
                var uploadPartRequest = new UploadPartRequest()
                {
                    BucketName = BucketName,
                    Key = file.Name,
                    UploadId = initiateResponse.UploadId,
                    PartNumber = i,
                    PartSize = partSize,
                    FilePath = file.FullName,
                    FilePosition = filePosition,
                    DisablePayloadSigning = true,
                    StreamTransferProgress = (sender, progressArgs) =>
                    {
                        progress?.Report((double)(progressArgs.TransferredBytes + position) / file.Length * 100);
                    }
                };

                uploadedParts.Add(await _client.UploadPartAsync(uploadPartRequest));
                filePosition += partSize;

            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Aborting :: ERROR: {Markup.Escape(ex.Message)}[/]");
            
            var abortUploadRequest = new AbortMultipartUploadRequest()
            {
                BucketName = BucketName,
                Key = file.Name,
                UploadId = initiateResponse.UploadId,
            };
            
            var abortResponse = await _client.AbortMultipartUploadAsync(abortUploadRequest);

            if (!abortResponse.HttpStatusCode.IsSuccessStatus())
            {
                AnsiConsole.MarkupLine("[red]Failed to abort upload[/]");
                return false;
            }
            
            AnsiConsole.MarkupLine("[yellow]Upload aborted[/]");
            return false;
        }

        var completeRequest = new CompleteMultipartUploadRequest()
        {
            BucketName = BucketName,
            Key = file.Name,
            UploadId = initiateResponse.UploadId,
        };
        
        completeRequest.AddPartETags(uploadedParts);
        
        var completeResponse = await _client.CompleteMultipartUploadAsync(completeRequest);

        if (completeResponse.HttpStatusCode.IsSuccessStatus())
        {
            AnsiConsole.MarkupLine("[green]Upload completed[/]");
            return true;
        }
        
        AnsiConsole.MarkupLine("[red]Failed to complete multipart upload[/]");
        return false;
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

        if (file.Length >= 5000000000)
        {
            return await MultiPartUpload(file, progress);
        }
        
        return await PutRequestUpload(file, progress);
    }
}