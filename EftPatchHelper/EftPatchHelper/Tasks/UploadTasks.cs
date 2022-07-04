using EftPatchHelper.Extensions;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;

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

        private async Task<bool> BuildUploadList()
        {
            var patcherFile = new FileInfo(_options.OutputPatchPath + ".zip");

            if (!patcherFile.Exists) return false;

            if (_settings.UsingMega() && _options.UploadToMega)
            {
                var mega = new MegaUpload(patcherFile, _settings.MegaEmail, _settings.MegaPassword);
                await mega.SetUploadFolder(_settings.MegaUploadFolder);
                _fileUploads.Add(mega);
            }

            if(_settings.UsingGoFile() && _options.UploadToGoFile)
            {
                var gofile = new GoFileUpload(patcherFile, _settings.GoFileApiKey);
                _fileUploads.Add(gofile);
            }

            return true;
        }

        private void CreateHubEntrySource()
        {
            var goFile = _fileUploads.SingleOrDefault(x => x.GetType() == typeof(GoFileUpload));
            var mega = _fileUploads.SingleOrDefault(x => x.GetType() == typeof(MegaUpload));

            if(goFile == null || mega == null)
            {
                AnsiConsole.WriteLine("Failed to get required info to create hub entry source");
                return;
            }

            var goFileLink = goFile.GetLink();
            var megaLink = mega.GetLink();

            if(goFileLink == null || megaLink == null)
            {
                AnsiConsole.WriteLine("Failed to get link for uploaded files");
                return;
            }

            string output = $"<p>Downgrade EFT Client files from version {_options.SourceClient.Version} to {_options.TargetClient.Version}</p>\n<p><br></p>";

            if(_options.UploadToGoFile)
            {
                output += $"\n<p><a href=\"{goFileLink}\">Download From GoFile</a></p>";
            }

            if(_options.UploadToMega)
            {
                output += $"\n<p><a href=\"{megaLink}\">Download From Mega</a></p>";
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
                    }

                    return true;
                });

            return succeeded;
        }

        public void Run()
        {
            if (!_options.CreateRelease)
            {
                UploadAllFiles().GetAwaiter().GetResult().ValidateOrExit();
            }

            CreateHubEntrySource();
        }
    }
}
