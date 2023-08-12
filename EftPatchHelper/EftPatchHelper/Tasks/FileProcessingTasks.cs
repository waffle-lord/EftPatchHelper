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

        public FileProcessingTasks(Settings settings, Options options)
        {
            _settings = settings;
            _options = options;
        }

        private bool BackupClients()
        {
            bool targetOK = _options.TargetClient.Backup(_settings, _options.IgnoreExistingDirectories);
            bool sourceOK = _options.SourceClient.Backup(_settings, _options.IgnoreExistingDirectories);

            return targetOK && sourceOK;
        }

        private bool CopyData(EftClient client, string message)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(message);

            var folderCopy = new FolderCopy(client.FolderPath, client.PrepPath);

            return folderCopy.Start(_options.IgnoreExistingDirectories);
        }

        private void CleanPrepFolders()
        {
            FolderCleaner.Clean(_options.TargetClient.PrepPath);
            FolderCleaner.Clean(_options.SourceClient.PrepPath);
        }

        public void Run()
        {
            AnsiConsole.Write(new Rule("Starting Tasks, this will take some time :)"));

            BackupClients().ValidateOrExit();

            CopyData(_options.SourceClient, "[gray]Copying[/] [blue]source[/][gray] to prep area ...[/]").ValidateOrExit();
            CopyData(_options.TargetClient, "[gray]Copying[/] [blue]target[/][gray] to prep area ...[/]").ValidateOrExit();

            CleanPrepFolders();
        }
    }
}
