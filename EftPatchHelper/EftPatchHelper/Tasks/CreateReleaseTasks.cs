using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Gitea.Api;
using Gitea.Model;
using Gitea.Client;
using Spectre.Console;
using EftPatchHelper.Extensions;
using System.Text.Json;

namespace EftPatchHelper.Tasks
{
    public class CreateReleaseTasks : IReleaseCreator
    {
        private Settings _settings;
        private Options _options;

        public CreateReleaseTasks(Settings settings, Options options)
        {
            _settings = settings;
            _options = options;
        }

        private bool UploadMirrorList(FileInfo file)
        {
            var r2Uplaod = new R2Upload(_settings.R2ConnectedDomainUrl, _settings.R2ServiceUrl, _settings.R2AccessKeyId,
                _settings.R2SecretKeyId, _settings.R2BucketName);
            
            

            return true;
        }

        // private bool UploadAsset(FileInfo file, Release release, RepositoryApi repo)
        // {
        //     return AnsiConsole.Status().Spinner(Spinner.Known.Point).Start("Uploading Asset", (StatusContext context) =>
        //     {
        //         AnsiConsole.MarkupLine($"[blue]Adding release asset: {file.Name.EscapeMarkup()}[/]");
        //
        //         file.Refresh();
        //
        //         if (!file.Exists)
        //         {
        //             AnsiConsole.MarkupLine($"[red]File does not exist: {file.FullName}[/]");
        //         }
        //
        //         using var fileStream = file.OpenRead();
        //
        //         try
        //         {
        //             var attachment = repo.RepoCreateReleaseAttachment(_settings.GiteaReleaseRepoOwner, _settings.GiteaReleaseRepoName, release.Id, fileStream, file.Name);
        //
        //             AnsiConsole.MarkupLine("[green]Upload Complete[/]");
        //
        //             return true;
        //         }
        //         catch (Exception ex)
        //         {
        //             AnsiConsole.MarkupLine("[red]Failed to upload asset[/]");
        //
        //             AnsiConsole.WriteException(ex);
        //
        //             return false;
        //         }
        //     });
        // }

        private Release? MakeRelease(RepositoryApi repo)
        {
            AnsiConsole.Write("Adding release to gitea ... ");

            string sourceTail = _options.SourceClient.Version.Split('.').Last();

            string targetTail = _options.TargetClient.Version.Split('.').Last();

            string releaseName = $"{sourceTail} to {targetTail}";

            string tag = $"{sourceTail}_{new Random().Next(100, 999)}";

            try
            {
                var release = repo.RepoCreateRelease(_settings.GiteaReleaseRepoOwner, _settings.GiteaReleaseRepoName, new CreateReleaseOption(null, false, releaseName, false, tag, null)); 

                AnsiConsole.MarkupLine($"[green]Release added: {release.Name.EscapeMarkup()}[/]");

                return release;
            }
            catch(Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to create release[/]");

                AnsiConsole.WriteException(ex);

                return null;
            }
        }

        public bool CreateMirrorList(FileInfo mirrorListFileInfo)
        {
            List<DownloadMirror> mirrors = _options.MirrorList.Values.ToList();

            string json = JsonSerializer.Serialize(mirrors, new JsonSerializerOptions() { WriteIndented = true });

            File.WriteAllText(mirrorListFileInfo.FullName, json);

            mirrorListFileInfo.Refresh();

            return mirrorListFileInfo.Exists;
        }

        public void Run()
        {
            AnsiConsole.WriteLine();

            var fileInfo = new FileInfo(Path.Join(Environment.CurrentDirectory, "mirrors.json"));

            CreateMirrorList(fileInfo);

            if (!_options.CreateRelease) return;

            var repo = new RepositoryApi(Configuration.Default);

            var release = MakeRelease(repo).ValidateOrExit<Release>();

            //UploadAsset(fileInfo, release, repo);
        }
    }
}
