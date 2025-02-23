// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using EftPatchHelper.EftInfo;
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
    public enum RunOption
    {
        MirrorTemplate,
        PizzaOvenApi,
        FileHash,
        UploadOnly,
        GeneratePatches,
    }

    public enum PizzaApiOption
    {
        GetCurrent,
        NewOrder,
        UpdateCurrent,
        Done,
    }
    
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
        PizzaHelper _pizzaHelper;
        FileHelper _fileHelper;
        Settings _settings;
        Options _options;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("EFT Patch Helper").Centered().Color(Color.Blue));
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AnsiConsole.Write(new Rule($"[purple]v{version}[/]").Centered().RuleStyle("blue"));

            try
            {
                var host = ConfigureHost(args);
                
                CheckUploads(host);
                
                host.Services.GetRequiredService<Program>().Run();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            }

            AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
            Console.ReadLine();
        }

        public static void CheckUploads(IHost host)
        {
            var r2Helper = host.Services.GetRequiredService<R2Helper>();

            var uploads = r2Helper.ListMultiPartUploads().GetAwaiter().GetResult();

            if (uploads.Count == 0)
            {
                AnsiConsole.MarkupLine("[green]No multi-part uploads found[/]");
                return;
            }

            AnsiConsole.MarkupLine("[red]multi-part uploads found[/]");
            foreach (var upload in uploads)
            {
                AnsiConsole.MarkupLine(
                    $"[blue]{Markup.Escape(upload.Key)}[/]");
                
                r2Helper.AbortMultipartUpload(upload.Key, upload.UploadId).GetAwaiter().GetResult();
            }
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
            IMirrorUploader uploadMirrorList,
            PizzaHelper pizzaHelper,
            FileHelper fileHelper,
            Settings settings,
            Options options
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
            _pizzaHelper = pizzaHelper;
            _fileHelper = fileHelper;
            _settings = settings;
            _options = options;
        }

        private FileInfo? CheckExistingPatchFile()
        {
            var patcher = new FileInfo(_settings.PatcherEXEPath);

            if (!patcher.Exists)
            {
                return null;
            }

            return patcher.Directory?.GetFiles("*.7z", SearchOption.TopDirectoryOnly).FirstOrDefault();
        }

        private void CreateExampleMirrorsFile()
        {
            var mirrorFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads\\mirror.json");
                    
            var examplePatchInfo = new PatchInfo()
            {
                SourceClientVersion = 445566,
                TargetClientVersion = 112233,
                Mirrors = new List<DownloadMirror>()
                {
                    new DownloadMirror()
                    {
                        Link = "http://yourmom.no-u",
                        Hash = "hash here",
                    }
                }
            };
                    
            string json = JsonSerializer.Serialize(examplePatchInfo, new JsonSerializerOptions() { WriteIndented = true });
                    
            File.WriteAllText(mirrorFilePath, json);
                    
            AnsiConsole.MarkupLine($"[green]Example File Created: {mirrorFilePath}[/]");
        }
        
        private void ConfirmOptions()
        {
            _options.IgnoreExistingDirectories = new ConfirmationPrompt("Skip existing directories? (you will be prompted if no)").Show(AnsiConsole.Console);

            if (_settings.UsingPizzaOven())
            {
                _options.UpdatePizzaStatus = new ConfirmationPrompt("Update pizza-oven status page?").Show(AnsiConsole.Console);
            }

            if (_settings.UsingMega())
            {
                _options.UploadToMega = new ConfirmationPrompt("Upload to Mega?").Show(AnsiConsole.Console);
            }

            if (_settings.UsingGoFile())
            {
                _options.UploadToGoFile = new ConfirmationPrompt("Upload to GoFile?").Show(AnsiConsole.Console);
            }

            if (_settings.UsingR2())
            {
                _options.UplaodToR2 = new ConfirmationPrompt($"Upload to R2 ({_settings.R2BucketName})?").Show(AnsiConsole.Console);
            }

            if (_settings.SftpUploads.Count > 0)
            {
                _options.UploadToSftpSites =
                    new ConfirmationPrompt($"Upload to SFTP sites? ( {_settings.SftpUploads.Count} sites )").Show(AnsiConsole.Console);
            }
        }

        private void ComputeFileHash(FileInfo? file)
        {
            if (file is not { Exists: true })
            {
                AnsiConsole.MarkupLine("[red]File not found[/]");
            }
            
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .Start("Computing hash ...",
                    _ =>
                    {
                        AnsiConsole.MarkupLine(
                            $"File Hash: [blue]{Markup.Escape(_fileHelper.GetFileHash(file))}[/]");
                    });
        }

        private bool SetupUploadOnly(FileInfo? file)
        {
            if (file is not { Exists: true })
            {
                return false;
            }

            // example output file name: Patcher_16.0.0.34447_to_15.5.1.33420.7z
            var versions = file.Name.Replace(".7z", "").Split('_');

            if (versions.Length != 4)
            {
                return false;
            }

            // Patcher | **16.0.0.34447** | to | 15.5.1.33420
            var sourceVersion = versions[1];

            // Patcher | 16.0.0.34447 | to | **15.5.1.33420**
            var targetVersion = versions[3];

            _options.OutputPatchPath = file.FullName.Replace(".7z", "");

            _options.SourceClient = new EftClient()
            {
                Version = sourceVersion,
            };

            _options.TargetClient = new EftClient()
            {
                Version = targetVersion,
            };

            return true;
        }

        private void ProcessPizzaApiDirect()
        {
            if (!_settings.UsingPizzaOven())
            {
                return;
            }

            var apiOptionsSelection = new SelectionPrompt<PizzaApiOption>()
                .Title("Select an option to run")
                .AddChoices(Enum.GetValues<PizzaApiOption>())
                .UseConverter(x =>
                {
                    return x switch
                    {
                        PizzaApiOption.GetCurrent => "Get current order",
                        PizzaApiOption.UpdateCurrent => "Update current order",
                        PizzaApiOption.NewOrder => "Create new order",
                        PizzaApiOption.Done => "Exit",
                        _ => "--error--"
                    };
                });

            PizzaApiOption answer;
            do
            {
                AnsiConsole.Clear();
                answer = AnsiConsole.Prompt(apiOptionsSelection);

                switch (answer)
                {
                    case PizzaApiOption.GetCurrent:
                        var currentOrder = _pizzaHelper.GetCurrentOrder();

                        if (currentOrder is null)
                        {
                            AnsiConsole.MarkupLine("[purple]No open order found[/]");
                            break;
                        }

                        AnsiConsole.MarkupLine("=== Current Open Order ===");
                        AnsiConsole.MarkupLine($"Order #  : [blue]{currentOrder.OrderNumber}[/]");
                        AnsiConsole.MarkupLine($"Message  : [blue]{currentOrder.Message.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"Progress : [blue]{currentOrder.Progress}[/]");
                        break;
                    case PizzaApiOption.NewOrder:
                        var newOrder = PizzaOrderData.PromptCreate();
                        if (_pizzaHelper.PostNewOrder(newOrder) == null)
                        {
                            AnsiConsole.MarkupLine("[red]Failed to create new order[/]");
                            break;
                        }

                        AnsiConsole.MarkupLine("[green]Order create[/]");
                        break;
                    case PizzaApiOption.UpdateCurrent:
                        currentOrder = _pizzaHelper.GetCurrentOrder();

                        if (currentOrder == null)
                        {
                            AnsiConsole.MarkupLine("[purple]No current order to update[/]");
                            break;
                        }

                        AnsiConsole.MarkupLine("=== Current Open Order ===");
                        AnsiConsole.MarkupLine($"Order #  : [blue]{currentOrder.OrderNumber}[/]");
                        AnsiConsole.MarkupLine($"Message  : [blue]{currentOrder.Message.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"Progress : [blue]{currentOrder.Progress}[/]");
                        AnsiConsole.Write(new Rule());

                        var updatedOrder = PizzaOrderData.PromptUpdate(currentOrder);

                        if (_pizzaHelper.UpdateOrder(currentOrder.Id, updatedOrder))
                        {
                            AnsiConsole.MarkupLine("[green]Order updated[/]");
                            break;
                        }

                        AnsiConsole.MarkupLine("[red]Failed to update order[/]");
                        break;
                    case PizzaApiOption.Done:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                AnsiConsole.MarkupLine("Press [blue]Enter[/] to continue ...");
                Console.ReadLine();
            }
            while(answer != PizzaApiOption.Done);
        }

        public void Run()
        {
            _settingsTasks.Run();
            
            var existingPatchFile = CheckExistingPatchFile();

            var fileName = existingPatchFile != null ? existingPatchFile.Name : "n/a";
            
            var runOptionsSelection = new SelectionPrompt<RunOption>()
                .Title("Select an option to run")
                .AddChoices(Enum.GetValues<RunOption>())
                .UseConverter(x =>
                {
                    return x switch
                    {
                        RunOption.MirrorTemplate => "Create mirror.json template",
                        RunOption.PizzaOvenApi => "Pizza Oven API",
                        RunOption.FileHash => $"Get file hash: {fileName}",
                        RunOption.UploadOnly => $"Upload Existing File: {fileName}",
                        RunOption.GeneratePatches => "Generate patches",
                        _ => "--error--"
                    };
                });
                
            var answer = AnsiConsole.Prompt(runOptionsSelection);

            if (answer is RunOption.GeneratePatches or RunOption.UploadOnly)
            {
                ConfirmOptions();
            }

            switch (answer)
            {
                case RunOption.MirrorTemplate:
                    CreateExampleMirrorsFile();
                    break;
                case RunOption.PizzaOvenApi:
                    ProcessPizzaApiDirect();
                    break;
                case RunOption.FileHash:
                    ComputeFileHash(existingPatchFile);
                    break;
                case RunOption.GeneratePatches:
                    
                    _clientSelectionTasks.Run();

                    PizzaOrder? order = null;
                    
                    if (_settings.UsingPizzaOven() && _options.UpdatePizzaStatus)
                    {
                        var currentOrder = _pizzaHelper.GetCurrentOrder();

                        if (currentOrder != null)
                        {
                            var cancelOrder = PizzaOrderData.CancelOrder(currentOrder);

                            _pizzaHelper.UpdateOrder(currentOrder.Id, cancelOrder);
                        }
                        
                        if (!int.TryParse(_options.SourceClient.Version.Split('.').Last(), out var sourceVersion))
                        {
                            AnsiConsole.MarkupLine("[red]Failed to get source version. Please provide it manually[/]");
                            sourceVersion = new TextPrompt<int>("Enter source version: ").Show(AnsiConsole.Console);

                            if (sourceVersion < 1)
                            {
                                AnsiConsole.MarkupLine("[red]Invalid provided source version. Aborting.");
                                return;
                            }
                        }

                        var newOrder = new PizzaOrderData
                        {
                            OrderNumber = sourceVersion,
                            Message = "New order received! Cleaning up the kitchen",
                            StepLabels = "Setup,Patching,Testing,Compress,Upload",
                            CurrentStep = 0,
                            StepProgress = 0
                        };
                    
                        order = _pizzaHelper.PostNewOrder(newOrder);
                    }
                    
                    _cleanupTasks.Run();
                    
                    _fileProcessingTasks.Run();
                    
                    _patchGenTasks.Run();
                    
                    _patchTestingTasks.Run();
                    
                    _compressPatcherTasks.Run();
                    
                    _uploadTasks.Run();
                    _uploadMirrorList.Run();
                    
                    break;
                case RunOption.UploadOnly:
                    if (!SetupUploadOnly(existingPatchFile))
                    {
                        AnsiConsole.MarkupLine("[red]File not found[/]");
                        return;
                    }
                    _uploadTasks.Run();
                    _uploadMirrorList.Run();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IHost ConfigureHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                var client = new HttpClient() { Timeout = TimeSpan.FromHours(1) };
                
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
                services.AddSingleton<PizzaHelper>();

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