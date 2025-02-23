using System.Net.Http.Json;
using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Tasks
{
    public class ClientSelectionTask : IClientSelectionTask
    {
        private Settings _settings;
        private Options _options;
        private EftClientSelector _clientSelector;
        private HttpClient _http;

        public ClientSelectionTask(Settings settings, Options options, EftClientSelector clientSelector, HttpClient client)
        {
            _settings = settings;
            _options = options;
            _http = client;
            _clientSelector = clientSelector;
        }

        private bool ChangeSettingsTargetVersion(string currentReleaseVersion)
        {
            _options.TargetClient = _clientSelector.GetClientSelection("Select [yellow]Target[/] Version", currentReleaseVersion);

            AnsiConsole.WriteLine();
            ConfirmationPrompt changeVersion = new ConfirmationPrompt($"Update settings target version to use [purple]{_options.TargetClient.Version}[/]?");

            if (changeVersion.Show(AnsiConsole.Console))
            {
                _settings.TargetEftVersion = _options.TargetClient.Version;

                return _settings.Save();
            }

            return true;
        }

        private bool ConfirmExistingTargetVersion(string currentReleaseVersion)
        {
            _clientSelector.LoadClientList();

            _options.TargetClient = _clientSelector.GetClient(_settings.TargetEftVersion);

            ConfirmationPrompt confirmTarget = new ConfirmationPrompt($"Use version [purple]{_settings.TargetEftVersion}[/] {(_options.TargetClient.Version.EndsWith(currentReleaseVersion) ? " ([green]latest release[/])" : "")} as target?");

            // If client is null or requested change, return false to ensure change settings target is called.
            return _options.TargetClient == null || !confirmTarget.Show(AnsiConsole.Console);
        }

        private bool SelectSourceVersion()
        {
            _options.SourceClient = _clientSelector.GetClientSelection("Select [blue]Source[/] Version");

            return _options.SourceClient != null;
        }

        private string GetCurrentReleaseVersion()
        {
            return AnsiConsole.Status().Start("Starting...", async ctx =>
            {
                ctx.Spinner = Spinner.Known.Dots8;
                ctx.Status = "Getting latest release ...";
                
                var blah = await _http.GetAsync(_settings.LatestReleaseUrl);
                var release = await blah.Content.ReadFromJsonAsync<ReleaseInfo>();

                return release?.ClientVersion ?? "failed to get version :(";
            }).GetAwaiter().GetResult();
        }

        public void Run(PizzaOrder? order = null)
        {
            var currentReleaseVersion = GetCurrentReleaseVersion();

            if (ConfirmExistingTargetVersion(currentReleaseVersion))
            {
                ChangeSettingsTargetVersion(currentReleaseVersion).ValidateOrExit();
            }

            SelectSourceVersion().ValidateOrExit();
        }
    }
}

