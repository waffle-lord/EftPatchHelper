using EftPatchHelper.EftInfo;
using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Tasks
{
    public class FileProcessingTasks : IFileProcessingTasks
    {
        Settings _settings;
        Options _options;
        PizzaHelper _pizzaHelper;
        private PizzaOrder? _order;

        public FileProcessingTasks(Settings settings, Options options, PizzaHelper pizzaHelper)
        {
            _settings = settings;
            _options = options;
            _pizzaHelper = pizzaHelper;
        }

        private bool BackupClients(IProgress<int>? orderProgress)
        {
            bool targetOK = _options.TargetClient.Backup(_settings, _options.IgnoreExistingDirectories, orderProgress);
            bool sourceOK = _options.SourceClient.Backup(_settings, _options.IgnoreExistingDirectories, orderProgress);

            return targetOK && sourceOK;
        }

        private bool CopyData(EftClient client, string message, IProgress<int>? orderProgress)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(message);

            var folderCopy = new FolderCopy(client.FolderPath, client.PrepPath);

            return folderCopy.Start(_options.IgnoreExistingDirectories, orderProgress: orderProgress);
        }

        private void CleanPrepFolders()
        {
            FolderCleaner.Clean(_options.TargetClient.PrepPath);
            FolderCleaner.Clean(_options.SourceClient.PrepPath);
        }

        public void Run(PizzaOrder? order = null)
        {
            _order = order;
            
            var orderProgressHelper = new PizzaOrderProgressHelper(_pizzaHelper, 3, "Backing up some data");
            var orderProgress = order != null ? orderProgressHelper.GetProgressReporter(order, 0) : null;
            
            AnsiConsole.Write(new Rule("Starting Tasks, this will take some time :)"));
            
            BackupClients(orderProgress).ValidateOrExit();

            orderProgressHelper.IncrementPart("Copying source files to prep area");
            CopyData(_options.SourceClient, "[gray]Copying[/] [blue]source[/][gray] to prep area ...[/]", orderProgress).ValidateOrExit();
            
            orderProgressHelper.IncrementPart("Copying target files to prep area");
            CopyData(_options.TargetClient, "[gray]Copying[/] [blue]target[/][gray] to prep area ...[/]", orderProgress).ValidateOrExit();

            CleanPrepFolders();
        }
    }
}
