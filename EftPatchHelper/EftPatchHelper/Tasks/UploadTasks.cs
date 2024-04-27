using EftPatchHelper.Extensions;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;
using System.Security.Cryptography;

namespace EftPatchHelper.Tasks
{
    public class UploadTasks : IUploadTasks
    {
        private Options _options;
        private Settings _settings;
        private List<IFileUpload> _fileUploads = new List<IFileUpload>();
        private Dictionary<IFileUpload, ProgressTask> uploadTasks = new Dictionary<IFileUpload, ProgressTask>();

        public UploadTasks(Options options, Settings settings)
        {
            _options = options;
            _settings = settings;
        }

        private static string GetFileHash(FileInfo file)
        {
            using (MD5 md5Service = MD5.Create())
            using (var sourceStream = file.OpenRead())
            {
                byte[] sourceHash = md5Service.ComputeHash(sourceStream);

                return Convert.ToBase64String(sourceHash);
            }
        }

        private async Task<bool> BuildUploadList()
        {
            var patcherFile = new FileInfo(_options.OutputPatchPath + ".7z");

            if (!patcherFile.Exists)
            {
                return false;
            }
            
            AnsiConsole.WriteLine("Building mirrors list ...");

            if(_settings.UsingGoFile() && _options.UploadToGoFile)
            {
                var gofile = new GoFileUpload(patcherFile, _settings.GoFileApiKey, _settings.GoFileFolderId);
                _fileUploads.Add(gofile);
                AnsiConsole.WriteLine("Added GoFile");
            }

            if (_settings.UsingR2() && _options.UplaodToR2)
            {
                var r2 = new R2Upload(_settings.R2ConnectedDomainUrl, _settings.R2ServiceUrl, _settings.R2AccessKeyId,
                    _settings.R2SecretKeyId, _settings.R2BucketName);
                _fileUploads.Add(r2);
                AnsiConsole.WriteLine($"Added R2: {_settings.R2BucketName}");
            }

            if (_settings.SftpUploads.Count > 0 && _options.UploadToSftpSites)
            {
              foreach (var sftpInfo in _settings.SftpUploads)
              {
                if (!sftpInfo.IsValid())
                {
                  continue;
                }

                AnsiConsole.WriteLine($"Added SFTP: {sftpInfo.Hostname}");
                _fileUploads.Add(new SftpUpload(patcherFile, sftpInfo));
              }
            }

            if (_settings.UsingMega() && _options.UploadToMega)
            {
                var mega = new MegaUpload(patcherFile, _settings.MegaEmail, _settings.MegaPassword);
                await mega.SetUploadFolder(_settings.MegaUploadFolder);
                _fileUploads.Add(mega);
                AnsiConsole.WriteLine("Added MEGA");
            }

            return true;
        }

        private void CreateHubEntrySource()
        {
            string output = $"<p>Downgrade EFT Client files from version {_options.SourceClient.Version} to {_options.TargetClient.Version}</p>\n<p><br></p>";

            foreach (var pair in _options.MirrorList)
            {
                if (!pair.Value.AddHubEntry)
                {
                    continue;
                }
                
                var displayText = pair.Key;
                var link = pair.Value.Link;

                if(link.Contains("gofile.io/download/direct/"))
                {
                    // gofile direct link is only for the mirror list, the hub entry should use the normal link
                    link = link.Replace("gofile.io/download/direct/", "gofile.io/download/");
                }

                output += $"\n<p><a href=\"{link}\">{displayText}</a></p>";
            }

            AnsiConsole.WriteLine(output);

            var unixTimestamp = (int)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

            string outputPath = $"{Environment.CurrentDirectory}\\hubEntry_{unixTimestamp}.txt";
            File.WriteAllText(outputPath, output);
            
            if(File.Exists(outputPath))
            {
                AnsiConsole.MarkupLine($"[green]Hub Entry Source saved: {outputPath.EscapeMarkup()}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Hub Entry Source saved failed[/]");
            }
        }

        static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }
            
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private async Task<bool> UploadAllFiles()
        {
            if(!await BuildUploadList())
            {
                return false;
            }
            
            AnsiConsole.MarkupLine($"[blue]Starting {_fileUploads[0].UploadFileInfo.Name} uploads ...[/]");

            var succeeded = await AnsiConsole.Progress().Columns(
                new TaskDescriptionColumn() { Alignment = Justify.Left},
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(Spinner.Known.Dots2)
                ).StartAsync<bool>(async context => 
                {
                    foreach(var file in _fileUploads)
                    {
                        var task = context.AddTask($"[purple][[Pending]][/] {file.DisplayName} - [blue]{BytesToString(file.UploadFileInfo.Length)}[/]");
                        task.IsIndeterminate = true;
                        uploadTasks.Add(file, task);
                    }

                    foreach(var pair in uploadTasks)
                    {
                        // set the value of the progress task object
                        var progress = new System.Progress<double>((d) => pair.Value.Value = d);

                        pair.Value.IsIndeterminate = false;
                        pair.Value.Description = $"{pair.Key.DisplayName} - [blue]{BytesToString(pair.Key.UploadFileInfo.Length)}[/]";

                        if(!await pair.Key.UploadAsync(progress))
                        {
                            AnsiConsole.MarkupLine($"[red]{pair.Key.DisplayName.EscapeMarkup()} failed[/]");
                        }
                        else
                        {
                            DownloadMirror mirror = new DownloadMirror
                            {
                                AddHubEntry = pair.Key.AddHubEntry,
                                Link = pair.Key.GetLink(),
                                Hash = GetFileHash(pair.Key.UploadFileInfo)
                            };

                            _options.MirrorList.Add(pair.Key.HubEntryText, mirror);
                        }
                    }

                    return _options.MirrorList.Count > 0;
                });

            return succeeded;
        }

        public void Run()
        {
            if (!_options.UploadToGoFile && !_options.UploadToMega && !_options.UploadToSftpSites) return;

            UploadAllFiles().GetAwaiter().GetResult().ValidateOrExit();

            CreateHubEntrySource();
        }
    }
}
