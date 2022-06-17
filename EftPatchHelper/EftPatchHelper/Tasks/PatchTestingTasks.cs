using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;
using System.Diagnostics;

namespace EftPatchHelper.Tasks
{
    public class PatchTestingTasks : IPatchTestingTasks
    {
        private Settings _settings;
        private Options _options;

        public PatchTestingTasks(Settings settings, Options options)
        {
            _settings = settings;
            _options = options;
        }

        private bool CopySourceToPrep()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Re-copying [blue]source[/] to prep area for testing ...");

            var sourceCopy = new FolderCopy(_options.SourceClient.FolderPath, _options.SourceClient.PrepPath);

            return sourceCopy.Start(false, true);
        }

        private bool RunPatcher()
        {
            var patcherCopy = new FolderCopy(_options.OutputPatchPath, _options.SourceClient.PrepPath);

            patcherCopy.Start(false, true);

            return AnsiConsole.Status().Spinner(Spinner.Known.Shark).Start("Starting Patcher ...", (StatusContext context) =>
            {
                var patchProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Join(_options.SourceClient.PrepPath, "patcher.exe"),
                    ArgumentList = { "autoclose" },
                    WorkingDirectory = _options.SourceClient.PrepPath
                });

                context.Status = "Running Patcher Test ...";

                patchProcess.WaitForExit();


                switch ((PatcherExitCode)patchProcess.ExitCode)
                {
                    case PatcherExitCode.Success:
                        {
                            AnsiConsole.MarkupLine("[green]Patch Test Succeeded[/]");
                            return true;
                        }
                    case PatcherExitCode.NoPatchFolder:
                        {
                            AnsiConsole.MarkupLine("[red]No patch folder found[/]");
                            return false;
                        }
                    case PatcherExitCode.MissingFile:
                        {
                            AnsiConsole.MarkupLine("[red]Missing file[/]");
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
            CopySourceToPrep().ValidateOrExit();

            RunPatcher().ValidateOrExit();
        }
    }
}
