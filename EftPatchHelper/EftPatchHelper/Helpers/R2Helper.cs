using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Helpers;

public class R2Helper
{
    private readonly IAmazonS3? _client;
    public bool IsReady => _client != null;

    private Settings _settings;

    public R2Helper(Settings settings, Options options)
    {
        _settings = settings;

        if (_settings.UsingR2() && options.UplaodToR2)
        {
            var creds = new BasicAWSCredentials(_settings.R2AccessKeyId, _settings.R2SecretKeyId);
            _client = new AmazonS3Client(creds, new AmazonS3Config
            {
                ServiceURL = _settings.R2ServiceUrl,
            });
        }
    }

    /// <summary>
    /// Deletes all content in the bucket
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ClearBucketAsync()
    {
        AnsiConsole.MarkupLine($"[blue]Getting bucket contents: {_settings.R2BucketName}[/]");
        var listBucketReponse = await _client.ListObjectsAsync(_settings.R2BucketName);

        if (listBucketReponse.HttpStatusCode != HttpStatusCode.OK)
        {
            AnsiConsole.MarkupLine("[red]failed to get bucket contents[/]");
            return false;
        }
        
        AnsiConsole.MarkupLine("[blue]Removing old content");
        foreach (var s3Object in listBucketReponse.S3Objects)
        {
            var deleteRepsonse = await _client.DeleteObjectAsync(_settings.R2BucketName, s3Object.Key);

            if (deleteRepsonse.HttpStatusCode != HttpStatusCode.OK)
            {
                AnsiConsole.MarkupLine($"[red]failed to delete {_settings.R2BucketName}::{s3Object.Key}[/]");
                return false;
            }
            
            AnsiConsole.MarkupLine($"[green]{_settings.R2BucketName}::{s3Object.Key} removed[/]");
        }

        return true;
    }

    public async Task<bool> UplaodToBucketAsync(FileInfo file, IProgress<double> progress = null)
    {
        // todo: this
    }
}