// See https://aka.ms/new-console-template for more information
using EftPatchHelper;
using EftPatchHelper.EftInfo;
using EftPatchHelper.Helpers;
using Spectre.Console;
using System.Diagnostics;

Settings? settings = Settings.Load();


// check settings file exists
if(settings == null)
{
    settings = new Settings();
    settings.Save();

    AnsiConsole.MarkupLine($"Settings file was create here: \n[blue]{Settings.settingsFile}[/]\n\nPlease update it and try again.");
    AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
    Console.ReadLine();
    return;
}

// validate settings
if(!settings.Validate())
{
    AnsiConsole.MarkupLine($"[red]Settings file seems to be missing some information, please fix it[/]\n\nPath to file:\n[blue]{Settings.settingsFile}[/]\n\n");
    AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
    Console.ReadLine();
    return;
}

/// Fancy
AnsiConsole.Write(new FigletText("EFT Patch Helper").Centered().Color(Color.Blue));

// show some settings information
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"Current target version is [purple]{settings.TargetEftVersion}[/]");
AnsiConsole.MarkupLine($"Prep folder path is [purple]{settings.PrepFolderPath}[/]");
AnsiConsole.MarkupLine($"Backup folder path is [purple]{settings.BackupFolderPath}[/]");
AnsiConsole.WriteLine();

EftClientSelector.LoadClientList(settings);

EftClient targetClient = EftClientSelector.GetClient(settings.TargetEftVersion);
EftClient sourceClient;

AnsiConsole.WriteLine();
bool promptToOverwrite = new ConfirmationPrompt("Prompt to overwrite directories?").Show(AnsiConsole.Console);

AnsiConsole.WriteLine();
ConfirmationPrompt confirmTarget = new ConfirmationPrompt($"Use version [purple]{settings.TargetEftVersion}[/] as target?");

if (!confirmTarget.Show(AnsiConsole.Console) || targetClient == null)
{
    targetClient = EftClientSelector.GetClientSelection("Select [yellow]Target[/] Version");

    AnsiConsole.WriteLine();
    ConfirmationPrompt changeVersion = new ConfirmationPrompt($"Update settings target version to use [purple]{targetClient.Version}[/]?");

    if(changeVersion.Show(AnsiConsole.Console))
    {
        settings.TargetEftVersion = targetClient.Version;

        settings.Save();
    }
}

sourceClient = EftClientSelector.GetClientSelection("Select [blue]Source[/] Version");


//backup data if needed
targetClient.Backup(settings, !promptToOverwrite);
sourceClient.Backup(settings, !promptToOverwrite);

//copy source to prep directory
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[gray]Copying[/] [blue]source[/][gray] to prep area ...[/]");

FolderCopy sourceCopy = new FolderCopy(sourceClient.FolderPath, sourceClient.PrepPath);

sourceCopy.Start(!promptToOverwrite);

//copy target to prep directory
AnsiConsole.MarkupLine("[gray]Copying[/] [blue]target[/][gray] to prep area ...[/]");

FolderCopy targetCopy = new FolderCopy(targetClient.FolderPath, targetClient.PrepPath);

targetCopy.Start(!promptToOverwrite);

// clean prep source and target folders of uneeded data
FolderCleaner.Clean(sourceClient.PrepPath);

FolderCleaner.Clean(targetClient.PrepPath);

// start patcher
if(File.Exists(settings.PatcherEXEPath))
{
    string patcherOutputName = $"Patcher_{sourceClient.Version}_to_{targetClient.Version}";

    AnsiConsole.Markup("Starting patcher ... ");

    Process.Start(new ProcessStartInfo()
    {
        FileName = settings.PatcherEXEPath,
        WorkingDirectory = new FileInfo(settings.PatcherEXEPath).Directory?.FullName ?? Directory.GetCurrentDirectory(),
        ArgumentList = {$"OutputFolderName::{patcherOutputName}", $"SourceFolderPath::{sourceClient.PrepPath}", $"TargetFolderPath::{targetClient.PrepPath}", $"AutoZip::{settings.AutoZip}"}
    });
}

AnsiConsole.MarkupLine("[green]done[/]");

AnsiConsole.WriteLine();

// done
AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");

Console.ReadLine();