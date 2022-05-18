// See https://aka.ms/new-console-template for more information
using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using EftPatchHelper.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace EftPatchHelper
{
    public class Program
    {
        ITaskable _settingsTasks;
        ITaskable _clientSelectionTasks;

        public static void Main(string[] args)
        {
            // Fancy
            AnsiConsole.Write(new FigletText("EFT Patch Helper").Centered().Color(Color.Blue));

            var host = ConfigureHost(args);
            host.Services.GetRequiredService<Program>().Run();
        }

        public Program(
            ISettingsTask settingsTasks,
            IClientSelectionTask clientSelectionTasks
            )
        {
            _settingsTasks = settingsTasks;
            _clientSelectionTasks = clientSelectionTasks;
        }

        public void Run()
        {
            _settingsTasks.Run().ValidateOrExit();
            _clientSelectionTasks.Run().ValidateOrExit();
        }

        private static IHost ConfigureHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                services.AddSingleton<Options>();
                services.AddSingleton<Settings>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    var settings = configuration.Get<Settings>();
                    if (settings == null) throw new Exception("Failed to retrieve settings");
                    return settings;
                });

                services.AddScoped<EftClientSelector>();

                services.AddTransient<ISettingsTask, StartupSettingsTask>();
                services.AddTransient<IClientSelectionTask, ClientSelectionTask>();
                services.AddTransient<Program>();
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile(Settings.settingsFile, optional: true, reloadOnChange: true);
            })
            .Build();
        }
    }
}

//EftClientSelector.LoadClientList(settings);

//EftClient targetClient = EftClientSelector.GetClient(settings.TargetEftVersion);
//EftClient sourceClient;

//AnsiConsole.WriteLine();
//ConfirmationPrompt confirmTarget = new ConfirmationPrompt($"Use version [purple]{_settings.TargetEftVersion}[/] as target?");

//if (!confirmTarget.Show(AnsiConsole.Console) || targetClient == null)
//{
//    targetClient = EftClientSelector.GetClientSelection("Select [yellow]Target[/] Version");

//    AnsiConsole.WriteLine();
//    ConfirmationPrompt changeVersion = new ConfirmationPrompt($"Update settings target version to use [purple]{targetClient.Version}[/]?");

//    if (changeVersion.Show(AnsiConsole.Console))
//    {
//        settings.TargetEftVersion = targetClient.Version;

//        settings.Save();
//    }
//}

//sourceClient = EftClientSelector.GetClientSelection("Select [blue]Source[/] Version");


////backup data if needed
//targetClient.Backup(settings, !promptToOverwrite);
//sourceClient.Backup(settings, !promptToOverwrite);

////copy source to prep directory
//AnsiConsole.WriteLine();
//AnsiConsole.MarkupLine("[gray]Copying[/] [blue]source[/][gray] to prep area ...[/]");

//FolderCopy sourceCopy = new FolderCopy(sourceClient.FolderPath, sourceClient.PrepPath);

//sourceCopy.Start(!promptToOverwrite);

////copy target to prep directory
//AnsiConsole.MarkupLine("[gray]Copying[/] [blue]target[/][gray] to prep area ...[/]");

//FolderCopy targetCopy = new FolderCopy(targetClient.FolderPath, targetClient.PrepPath);

//targetCopy.Start(!promptToOverwrite);

//// clean prep source and target folders of uneeded data
//FolderCleaner.Clean(sourceClient.PrepPath);

//FolderCleaner.Clean(targetClient.PrepPath);

//// start patcher
//if(File.Exists(settings.PatcherEXEPath))
//{
//    string patcherOutputName = $"Patcher_{sourceClient.Version}_to_{targetClient.Version}";

//    AnsiConsole.Markup("Starting patcher ... ");

//    var genProc = Process.Start(new ProcessStartInfo()
//    {
//        FileName = settings.PatcherEXEPath,
//        WorkingDirectory = new FileInfo(settings.PatcherEXEPath).Directory?.FullName ?? Directory.GetCurrentDirectory(),
//        ArgumentList = 
//        {
//            $"OutputFolderName::{patcherOutputName}",
//            $"SourceFolderPath::{sourceClient.PrepPath}",
//            $"TargetFolderPath::{targetClient.PrepPath}",
//            $"AutoZip::{settings.AutoZip}",
//            $"AutoClose::{settings.AutoClose}"
//        }
//    });

//    genProc?.WaitForExit();

//    switch((PatcherExitCode)genProc.ExitCode)
//    {
//        case PatcherExitCode.ProgramClosed:
//            {

//                break;
//            }
//        case PatcherExitCode.Success:
//            {

//                break;
//            }
//        case PatcherExitCode.MissingDir:
//            {

//                break;
//            }
//        default:
//            {
//                break;
//            }
//    }
//}

//AnsiConsole.MarkupLine("[green]done[/]");

//AnsiConsole.WriteLine();

//// done
//AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");

//Console.ReadLine();