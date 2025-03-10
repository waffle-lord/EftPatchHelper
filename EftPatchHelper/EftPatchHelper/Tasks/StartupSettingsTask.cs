﻿using EftPatchHelper.Extensions;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
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

        public void Run()
        {
            ValidateSettings().ValidateOrExit();

            PrintSummary();
        }
    }
}