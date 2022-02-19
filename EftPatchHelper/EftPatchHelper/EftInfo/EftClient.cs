using EftPatchHelper.Helpers;
using Spectre.Console;

namespace EftPatchHelper.EftInfo
{
    public class EftClient
    {
        public string DisplayName => $"{Location}: {Version}";
        public string Version { get; set; }
        public string FolderPath { get; set; }
        public string PrepPath { get; set; }
        public EftClientLocation Location { get; set; }

        public bool Backup(Settings settings, bool IgnoreIfexists = false)
        {
            string backupPath = Path.Join(settings.BackupFolderPath, Version);

            if (Directory.Exists(backupPath) && Location != EftClientLocation.Live) return true;

            AnsiConsole.MarkupLine($"[blue]Backing up {Version} ...[/]");

            FolderCopy backup = new FolderCopy(FolderPath, backupPath);

            return backup.Start(IgnoreIfexists);
        }
    }
}
