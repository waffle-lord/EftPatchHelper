using EftPatchHelper.Extensions;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Gitea.Client;
using Spectre.Console;

namespace EftPatchHelper.Tasks
{
    public class StartupSettingsTask : ISettingsTask
    {
        private Settings? _settings { get; set; }
        private Options _options { get; set; }
        public StartupSettingsTask(Settings? settings, Options options)
        {
            _settings = settings;
            _options = options;
        }

        public void PrintSummary()
        {
            AnsiConsole.WriteLine();

            // show some settings information
            Table settingsTable = new Table()
                      .Alignment(Justify.Center)
                      .HorizontalBorder()
                      .HideHeaders()
                      .BorderStyle(Style.Parse("blue"))
                      .AddColumn("Data")
                      .AddColumn("Value")
                      .AddRow("Current target version", $"[purple]{_settings?.TargetEftVersion}[/]")
                      .AddRow("Prep folder path", $"[purple]{_settings?.PrepFolderPath}[/]")
                      .AddRow("Backup folder path", $"[purple]{_settings?.BackupFolderPath}[/]");

            AnsiConsole.Write(settingsTable);

            AnsiConsole.WriteLine();
        }

        private bool ValidateSettings()
        {
            // check settings file exists
            if (_settings == null)
            {
                _settings = new Settings();
                _settings.Save();

                AnsiConsole.MarkupLine($"Settings file was create here: \n[blue]{Settings.settingsFile}[/]\n\nPlease update it and try again.");
                return false;
            }

            // validate settings
            if (!_settings.Validate())
            {
                AnsiConsole.MarkupLine($"[red]Settings file seems to be missing some information, please fix it[/]\n\nPath to file:\n[blue]{Settings.settingsFile}[/]\n\n");
                return false;
            }

            return true;
        }

        private void ConfirmOptions()
        {
            _options.IgnoreExistingDirectories = new ConfirmationPrompt("Skip existing directories? (you will be prompted if no)").Show(AnsiConsole.Console);

            if (_settings.UsingGitea())
            {
                Configuration.Default.BasePath = _settings.GiteaApiBasePath;
                Configuration.Default.AddApiKey("token", _settings.GiteaApiKey);

                _options.CreateRelease = new ConfirmationPrompt("Create a release on gitea?").Show(AnsiConsole.Console);
            }

            if (_settings.UsingMega())
            {
                _options.UploadToMega = new ConfirmationPrompt("Upload to Mega?").Show(AnsiConsole.Console);
            }

            if (_settings.UsingGoFile())
            {
                _options.UploadToGoFile = new ConfirmationPrompt("Upload to GoFile?").Show(AnsiConsole.Console);
            }

            if (_settings.SftpUploads.Count > 0)
            {
                _options.UploadToSftpSites =
                    new ConfirmationPrompt($"Upload to SFTP sites? ( {_settings.SftpUploads.Count} sites )").Show(AnsiConsole.Console);
            }
        }

        public void Run()
        {
            ValidateSettings().ValidateOrExit();

            ConfirmOptions();

            PrintSummary();
        }
    }
}