// See https://aka.ms/new-console-template for more information
using EftPatchHelper;
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

// Source Selection Prompt
SelectionPrompt<string> sourcePrompt = new SelectionPrompt<string>()
{
    Title = "Select Source Version",
    MoreChoicesText = "Move cursor to see more versions",
    PageSize = 10
};

// Get eft live version
var eftVersion = FileVersionInfo.GetVersionInfo(Path.Join(settings.LiveEftPath, "EscapeFromTarkov.exe")).ProductVersion?.Replace('-', '.');

if(eftVersion != null)
{
    //remove leading 0 from version number
    if (eftVersion.StartsWith("0."))
    {
        eftVersion = eftVersion.Remove(0, 2);
    }

    // add eft liver version to selection prompt choices
    sourcePrompt.AddChoice($"Live: {eftVersion}");
}

// add backup folders to source prompt choices
foreach(string backup in Directory.GetDirectories(settings.BackupFolderPath))
{
    DirectoryInfo backupDir = new DirectoryInfo(backup);

    if(!backupDir.Exists)
    {
        continue;
    }

    sourcePrompt.AddChoice($"Backup: {backupDir.Name}");
}


string result = sourcePrompt.Show(AnsiConsole.Console);

string sourceVersion = result.Split(": ")[1];

string sourceBackupPath = Path.Join(settings.BackupFolderPath, sourceVersion);

//backup live folder if it was selected
if(result.StartsWith("Live:"))
{
    // only backup the live folder if it doesn't exist already
    if(!Directory.Exists(sourceBackupPath))
    {
        AnsiConsole.MarkupLine("[blue]Backing up live ...[/]");
        FolderCopy backupLiveCopy = new FolderCopy(settings.LiveEftPath, sourceBackupPath);

        backupLiveCopy.Start();
    }
}

string targetBackupPath = Path.Join(settings.BackupFolderPath, settings.TargetEftVersion);

string targetPrepPath = Path.Join(settings.PrepFolderPath, settings.TargetEftVersion);

string sourcePrepPath = Path.Join(settings.PrepFolderPath, sourceVersion);


//copy source to prep directory
AnsiConsole.MarkupLine("[gray]Copying[/] [blue]source[/][gray] to prep area ...[/]");

FolderCopy sourceCopy = new FolderCopy(sourceBackupPath, sourcePrepPath);

sourceCopy.Start();

//copy target to prep directory
AnsiConsole.MarkupLine("[gray]Copying[/] [blue]target[/][gray] to prep area ...[/]");

FolderCopy targetCopy = new FolderCopy(targetBackupPath, targetPrepPath);

targetCopy.Start();

// clean prep source and target folders of uneeded data
FolderCleaner.Clean(sourcePrepPath);

FolderCleaner.Clean(targetPrepPath);

// start patcher
if(File.Exists(settings.PatcherEXEPath))
{
    string patcherOutputName = $"Patcher_{sourceVersion}_to_{settings.TargetEftVersion}";

    AnsiConsole.Markup("Starting patcher ... ");

    Process.Start(new ProcessStartInfo()
    {
        FileName = settings.PatcherEXEPath,
        WorkingDirectory = new FileInfo(settings.PatcherEXEPath).Directory?.FullName ?? Directory.GetCurrentDirectory(),
        ArgumentList = {$"OutputFolderName::{patcherOutputName}", $"SourceFolderPath::{sourcePrepPath}", $"TargetFolderPath::{targetPrepPath}", $"AutoZip::{settings.AutoZip}"}
    });
}

AnsiConsole.MarkupLine("[green]done[/]");

AnsiConsole.WriteLine();

// done
AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");

Console.ReadLine();