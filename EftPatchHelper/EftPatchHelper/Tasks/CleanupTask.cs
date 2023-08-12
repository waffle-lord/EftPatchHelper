using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Tasks
{
    public class CleanupTask : ICleanupTask
    {
        private Settings _settings;
        private Options _options;

        List<FileSystemInfo> _fileToRemove = new List<FileSystemInfo>();

        public CleanupTask(Settings settings, Options options)
        {
            _settings = settings;
            _options = options;
        }

        private void GetPathsToRemove()
        {
            var prepFolders = Directory.GetDirectories(_settings.PrepFolderPath, "*");

            foreach (var prepFolder in prepFolders)
            {
                if (prepFolder == _options.TargetClient.PrepPath)
                    continue;

                _fileToRemove.Add(new DirectoryInfo(prepFolder));
            }

            var mirrorsPath = Path.Join(Environment.CurrentDirectory, "mirrors.json");
            var hubentries = Directory.GetFiles(Environment.CurrentDirectory, "hubEntry_*.txt");

            if (File.Exists(mirrorsPath))
                _fileToRemove.Add(new FileInfo(mirrorsPath));

            _fileToRemove.AddRange(hubentries.Select(x => new FileInfo(x)));

            var patcherDir = new FileInfo(_settings.PatcherEXEPath).Directory;

            _fileToRemove.AddRange(patcherDir.GetFiles().Where(x => x.FullName != _settings.PatcherEXEPath));
            _fileToRemove.AddRange(patcherDir.GetDirectories("*", SearchOption.TopDirectoryOnly));
        }

        private void RemoveData(Table table)
        {
            AnsiConsole.Live(table).Start(ctx =>
            {
                for (int i = 0; i < _fileToRemove.Count; i++)
                {
                    table.UpdateCell(i, 0, "[white]Removing ...[/]");

                    ctx.Refresh();

                    var item = _fileToRemove[i];

                    if (item is DirectoryInfo dir)
                        dir.Delete(true);

                    if (item is FileInfo file)
                        file.Delete();

                    
                    table.UpdateCell(i, 0, item.Exists ? "[red]Exists[/]" : "[green]Removed[/]");
                    ctx.Refresh();
                }
            });
        }

        public void Run()
        {
            GetPathsToRemove();

            if (_fileToRemove.Count == 0)
                return;

            Table removableFilesTable = new Table()
                .Alignment(Justify.Center)
                .AddColumn("Status")
                .AddColumn("File Name")
                .AddColumn("Full Path")
                .BorderStyle(Style.Parse("blue"));

            foreach (var file in _fileToRemove)
            {
                removableFilesTable.AddRow("[grey]Exists[/]", file.Name, file.FullName);
            }

            var cursorPos = Console.GetCursorPosition();

            AnsiConsole.Write(removableFilesTable);


            var removeFiles = new ConfirmationPrompt("Run cleanup to remove files shown above?").Show(AnsiConsole.Console);

            Console.SetCursorPosition(cursorPos.Left, cursorPos.Top);

            if (removeFiles)
                RemoveData(removableFilesTable);
        }
    }
}
