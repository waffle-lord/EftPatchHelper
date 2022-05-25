using Spectre.Console;

namespace EftPatchHelper.Helpers
{
    public static class FolderCleaner
    {
        public static string cleanPathsFile = Path.Join(Directory.GetCurrentDirectory(), "removePaths.txt");

        public static string[] AssumedPaths =
        {
            "BattlEye",
            "cache",
            "Logs",
            "ConsistencyInfo",
            "EscapeFromTarkov_BE.exe",
            "Uninstall.exe",
            "UnityCrashHandler64.exe",
            "WinPixEventRuntime.dll"
        };

        public static void Clean(string FolderPath)
        {
            AnsiConsole.Status()
            .Spinner(Spinner.Known.Default)
            .Start($"Cleaning Folder ...", ctx =>
            {
                AnsiConsole.MarkupLine($"[blue]INFO:[/] [gray]Getting folders to remove for {FolderPath} ...[/]");
                if (!File.Exists(cleanPathsFile)) File.WriteAllLines(cleanPathsFile, AssumedPaths);

                string[] delPaths = File.ReadAllLines(cleanPathsFile);

                foreach (string delPath in delPaths)
                {
                    string fsItemToRemove = Path.Join(FolderPath, delPath);

                    FileSystemInfo fsInfo = Directory.Exists(fsItemToRemove) ? new DirectoryInfo(fsItemToRemove) : new FileInfo(fsItemToRemove);

                    if (fsInfo is DirectoryInfo dInfo)
                    {
                        dInfo.Delete(true);
                    }
                    else
                    {
                        fsInfo.Delete();
                    }


                    fsInfo.Refresh();

                    if (!fsInfo.Exists)
                    {
                        AnsiConsole.MarkupLine($"[blue]INFO:[/] [gray]Deleting {fsInfo.Name} ...[/] [green]OK[/]");
                        continue;
                    }

                    AnsiConsole.MarkupLine($"[blue]INFO:[/] [gray]Deleting {fsInfo.Name} ...[/] [red]Failed[/]");
                }
            });
        }
    }
}
