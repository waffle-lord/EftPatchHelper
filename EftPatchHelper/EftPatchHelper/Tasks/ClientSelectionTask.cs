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

        public ClientSelectionTask(Settings settings, Options options, EftClientSelector clientSelector)
        {
            _settings = settings;
            _options = options;
            _clientSelector = clientSelector;
        }

        private bool ChangeSettingsTargetVersion()
        {
            _options.TargetClient = _clientSelector.GetClientSelection("Select [yellow]Target[/] Version");

            AnsiConsole.WriteLine();
            ConfirmationPrompt changeVersion = new ConfirmationPrompt($"Update settings target version to use [purple]{_options.TargetClient.Version}[/]?");

            if (changeVersion.Show(AnsiConsole.Console))
            {
                _settings.TargetEftVersion = _options.TargetClient.Version;

                return _settings.Save();
            }

            return true;
        }

        private bool ConfirmExistingTargetVersion()
        {
            _clientSelector.LoadClientList();

            _options.TargetClient = _clientSelector.GetClient(_settings.TargetEftVersion);

            ConfirmationPrompt confirmTarget = new ConfirmationPrompt($"Use version [purple]{_settings.TargetEftVersion}[/] as target?");

            // If client is null or requested change, return false to ensure change settings target is called.
            return _options.TargetClient == null || !confirmTarget.Show(AnsiConsole.Console);
        }

        private bool SelectSourceVersion()
        {
            _options.SourceClient = _clientSelector.GetClientSelection("Select [blue]Source[/] Version");

            return _options.SourceClient != null;
        }

        public void Run()
        {
            if (ConfirmExistingTargetVersion())
            {
                ChangeSettingsTargetVersion().ValidateOrExit();
            }

            SelectSourceVersion().ValidateOrExit();
        }
    }
}

