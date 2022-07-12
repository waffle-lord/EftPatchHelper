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
            var patcherFile = new FileInfo(_options.OutputPatchPath + ".zip");

            if (!patcherFile.Exists) return false;

            if(_settings.UsingGoFile() && _options.UploadToGoFile)
            {
                var gofile = new GoFileUpload(patcherFile, _settings.GoFileApiKey);
                _fileUploads.Add(gofile);
            }

            if (_settings.UsingMega() && _options.UploadToMega)
            {
                var mega = new MegaUpload(patcherFile, _settings.MegaEmail, _settings.MegaPassword);
                await mega.SetUploadFolder(_settings.MegaUploadFolder);
                _fileUploads.Add(mega);
            }

            return true;
        }

        private void CreateHubEntrySource()
        {
            string output = $"<p>Downgrade EFT Client files from version {_options.SourceClient.Version} to {_options.TargetClient.Version}</p>\n<p><br></p>";

            foreach (var pair in _options.MirrorList)
            {
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

        private async Task<bool> UploadAllFiles()
        {
            if(!await BuildUploadList())
            {
                return false;
            }

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
                        var task = context.AddTask($"[purple][[Pending]][/] {file.DisplayName}");
                        task.IsIndeterminate = true;
                        uploadTasks.Add(file, task);
                    }

                    foreach(var pair in uploadTasks)
                    {
                        // set the value of the progress task object
                        var progress = new System.Progress<double>((d) => pair.Value.Value = d);

                        pair.Value.IsIndeterminate = false;
                        pair.Value.Description = pair.Key.DisplayName;

                        if(!await pair.Key.UploadAsync(progress))
                        {
                            AnsiConsole.MarkupLine($"[red]{pair.Key.DisplayName.EscapeMarkup()} failed[/]");
                            return false;
                        }
                        else
                        {
                            DownloadMirror mirror = new DownloadMirror()
                            {
                                Link = pair.Key.GetLink(),
                                Hash = GetFileHash(pair.Key.UploadFileInfo)
                            };

                            _options.MirrorList.Add(pair.Key.HubEntryText, mirror);
                        }
                    }

                    return true;
                });

            return succeeded;
        }

        public void Run()
        {
            if (!_options.UploadToGoFile && !_options.UploadToMega) return;

            UploadAllFiles().GetAwaiter().GetResult().ValidateOrExit();

            CreateHubEntrySource();
        }
    }
}
