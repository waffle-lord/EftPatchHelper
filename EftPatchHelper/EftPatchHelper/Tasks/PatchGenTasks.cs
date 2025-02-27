using EftPatchHelper.Extensions;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;
using System.Diagnostics;
using PizzaOvenApi;
using PizzaOvenApi.Model.PizzaRequests;

namespace EftPatchHelper.Tasks
{
    internal class PatchGenTasks : IPatchGenTasks
    {
        private Settings _settings;
        private Options _options;
        private PizzaApi _pizzaApi;

        public PatchGenTasks(Settings settings, Options options, PizzaApi pizzaApi)
        {
            _settings = settings;
            _options = options;
            _pizzaApi = pizzaApi;
        }

        private bool RunPatchGenerator()
        {
            if (!File.Exists(_settings.PatcherEXEPath))
            {
                AnsiConsole.WriteLine("Could not find patch generator exe");
                return false;
            }

            string patcherOutputName = $"Patcher_{_options.SourceClient.Version}_to_{_options.TargetClient.Version}";

            var patcherPath = new FileInfo(_settings.PatcherEXEPath)?.Directory?.FullName;

            if(patcherPath == null)
            {
                AnsiConsole.WriteLine("Could not find patch generator folder");
                return false;
            }

            _options.OutputPatchPath = Path.Join(patcherPath, patcherOutputName);

            return AnsiConsole.Status().Spinner(Spinner.Known.Shark).Start("Staring Patch Generator ...", (StatusContext context) =>
            {
                var genProc = Process.Start(new ProcessStartInfo()
                {
                    FileName = _settings.PatcherEXEPath,
                    WorkingDirectory = new FileInfo(_settings.PatcherEXEPath).Directory?.FullName ?? Directory.GetCurrentDirectory(),
                    ArgumentList =
                    {
                        $"OutputFolderName::{patcherOutputName}",
                        $"SourceFolderPath::{_options.SourceClient.PrepPath}",
                        $"TargetFolderPath::{_options.TargetClient.PrepPath}",
                        $"AutoZip::false",
                        $"AutoClose::{_settings.AutoClose}"
                    }
                });


                context.Status = "Generating patches ...";

                genProc?.WaitForExit();

                switch ((PatcherExitCode)genProc.ExitCode)
                {
                    case PatcherExitCode.ProgramClosed:
                        {
                            AnsiConsole.MarkupLine("[red]Program was closed[/]");
                            return false;
                        }
                    case PatcherExitCode.Success:
                        {
                            AnsiConsole.MarkupLine("[green]Patches Generated[/]");
                            return true;
                        }
                    case PatcherExitCode.MissingDir:
                        {
                            AnsiConsole.MarkupLine("[red]Missing Dir[/]");
                            return false;
                        }
                    default:
                        {
                            AnsiConsole.MarkupLine("[red]Something went wrong[/]");
                            return false;
                        }
                }
            });
        }

        public void Run()
        {
            var order = _pizzaApi.GetCurrentOrder();
            
            if (order != null)
            {
                var orderUpdate =
                    new UpdatePizzaOrderRequest("Generating some delicious patches", PizzaOrderStep.Patch, -1);

                _pizzaApi.UpdateOrder(order.Id, orderUpdate);
            }

            RunPatchGenerator().ValidateOrExit();
        }
    }
}