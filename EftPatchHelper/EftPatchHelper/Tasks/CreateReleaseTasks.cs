using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Gitea.Api;
using Gitea.Model;
using Gitea.Client;
using Spectre.Console;
using EftPatchHelper.Extensions;

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

        private bool UploadAsset(FileInfo file, Release release, RepositoryApi repo)
        {
            return AnsiConsole.Status().Start("Uploading Asset", (StatusContext context) =>
            {
                AnsiConsole.MarkupLine($"[blue]Adding release asset: {file.Name.EscapeMarkup()}[/]");

                file.Refresh();

                if (!file.Exists)
                {
                    AnsiConsole.MarkupLine($"[red]File does not exist: {file.FullName}[/]");
                }

                using var fileStream = file.OpenRead();

                try
                {
                    var attachment = repo.RepoCreateReleaseAttachment(_settings.GiteaReleaseRepoOwner, _settings.GiteaReleaseRepoName, release.Id, fileStream, file.Name);

                    AnsiConsole.MarkupLine("[green]Upload Complete[/]");

                    return true;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[red]Failed to upload asset[/]");

                    AnsiConsole.WriteException(ex);

                    return false;
                }
            });
        }

        private Release? MakeRelease(RepositoryApi repo)
        {
            AnsiConsole.Write("Adding release to gitea ... ");

            string sourceTail = _options.SourceClient.Version.Split('.').Last();

            string targetTail = _options.TargetClient.Version.Split('.').Last();

            string releaseName = $"{sourceTail} to {targetTail}";

            try
            {
                var release = repo.RepoCreateRelease(_settings.GiteaReleaseRepoOwner, _settings.GiteaReleaseRepoName, new CreateReleaseOption(null, false, releaseName, false, sourceTail, null)); 

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

        public void Run()
        {
            AnsiConsole.WriteLine();

            if (!_options.CreateRelease) return;

            Configuration.Default.BasePath = _settings.GiteaApiBasePath;

            Configuration.Default.AddApiKey("token", _settings.GiteaApiKey);

            var repo = new RepositoryApi(Configuration.Default);

            var release = MakeRelease(repo).ValidateOrExit<Release>();

            var fileInfo = new FileInfo(_options.OutputPatchPath + ".zip");

            UploadAsset(fileInfo, release, repo);
        }
    }
}
