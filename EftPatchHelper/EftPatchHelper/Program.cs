// See https://aka.ms/new-console-template for more information
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
        ITaskable _fileProcessingTasks;
        ITaskable _patchGenTasks;
        ITaskable _patchTestingTasks;
        ITaskable _createReleaseTasks;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("EFT Patch Helper").Centered().Color(Color.Blue));

            var host = ConfigureHost(args);
            host.Services.GetRequiredService<Program>().Run();

            AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
            Console.ReadLine();
        }

        public Program(
            ISettingsTask settingsTasks,
            IClientSelectionTask clientSelectionTasks,
            IFileProcessingTasks fileProcessingTasks,
            IPatchGenTasks patchGenTasks,
            IPatchTestingTasks patchTestingTasks,
            IReleaseCreator createReleaseTasks
            )
        {
            _settingsTasks = settingsTasks;
            _clientSelectionTasks = clientSelectionTasks;
            _fileProcessingTasks = fileProcessingTasks;
            _patchGenTasks = patchGenTasks;
            _patchTestingTasks = patchTestingTasks;
            _createReleaseTasks = createReleaseTasks;
        }

        public void Run()
        {
            _settingsTasks.Run();
            _clientSelectionTasks.Run();
            _fileProcessingTasks.Run();
            _patchGenTasks.Run();
            _patchTestingTasks.Run();
            _createReleaseTasks.Run();
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
                services.AddTransient<IFileProcessingTasks, FileProcessingTasks>();
                services.AddTransient<IPatchGenTasks, PatchGenTasks>();
                services.AddTransient<IPatchTestingTasks, PatchTestingTasks>();
                services.AddTransient<IReleaseCreator, CreateReleaseTasks>();
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