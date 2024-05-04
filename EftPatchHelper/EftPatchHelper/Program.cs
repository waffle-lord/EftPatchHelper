// See https://aka.ms/new-console-template for more information

using System.Reflection;
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
        ITaskable _cleanupTasks;
        ITaskable _fileProcessingTasks;
        ITaskable _patchGenTasks;
        ITaskable _patchTestingTasks;
        ITaskable _compressPatcherTasks;
        ITaskable _uploadTasks;
        ITaskable _uploadMirrorList;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("EFT Patch Helper").Centered().Color(Color.Blue));
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AnsiConsole.Write(new Rule($"[purple]v{version}[/]").Centered().RuleStyle("blue"));

            var host = ConfigureHost(args);
            host.Services.GetRequiredService<Program>().Run();

            AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
            Console.ReadLine();
        }

        public Program(
            ISettingsTask settingsTasks,
            IClientSelectionTask clientSelectionTasks,
            ICleanupTask cleanupTasks,
            IFileProcessingTasks fileProcessingTasks,
            IPatchGenTasks patchGenTasks,
            IPatchTestingTasks patchTestingTasks,
            ICompressPatcherTasks compressPatcherTasks,
            IUploadTasks uploadTasks,
            IMirrorUploader uploadMirrorList
            )
        {
            _settingsTasks = settingsTasks;
            _clientSelectionTasks = clientSelectionTasks;
            _cleanupTasks = cleanupTasks;
            _fileProcessingTasks = fileProcessingTasks;
            _patchGenTasks = patchGenTasks;
            _patchTestingTasks = patchTestingTasks;
            _compressPatcherTasks = compressPatcherTasks;
            _uploadMirrorList = uploadMirrorList;
            _uploadTasks = uploadTasks;
        }

        public void Run()
        {
            _settingsTasks.Run();
            _clientSelectionTasks.Run();
            _cleanupTasks.Run();
            _fileProcessingTasks.Run();
            _patchGenTasks.Run();
            _patchTestingTasks.Run();
            _compressPatcherTasks.Run();
            _uploadTasks.Run();
            _uploadMirrorList.Run();
        }

        private static IHost ConfigureHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                HttpClient client = new HttpClient() { Timeout = TimeSpan.FromHours(1) };
                
                services.AddSingleton<Options>();
                services.AddSingleton(client);
                services.AddSingleton<Settings>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    var settings = configuration.Get<Settings>();
                    if (settings == null) throw new Exception("Failed to retrieve settings");
                    return settings;
                });

                services.AddSingleton<FileHelper>();
                services.AddSingleton<ZipHelper>();
                services.AddSingleton<R2Helper>();

                services.AddScoped<EftClientSelector>();

                services.AddTransient<ISettingsTask, StartupSettingsTask>();
                services.AddTransient<ICleanupTask, CleanupTask>();
                services.AddTransient<IClientSelectionTask, ClientSelectionTask>();
                services.AddTransient<IFileProcessingTasks, FileProcessingTasks>();
                services.AddTransient<IPatchGenTasks, PatchGenTasks>();
                services.AddTransient<IPatchTestingTasks, PatchTestingTasks>();
                services.AddTransient<ICompressPatcherTasks, CompressPatcherTasks>();
                services.AddTransient<IUploadTasks, UploadTasks>();
                services.AddTransient<IMirrorUploader, UploadMirrorListTasks>();
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