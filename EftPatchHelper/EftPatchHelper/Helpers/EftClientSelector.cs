using EftPatchHelper.EftInfo;
using Spectre.Console;
using System.Diagnostics;

namespace EftPatchHelper.Helpers
{
    public static class EftClientSelector
    {
        private static List<EftClient> clientList = new List<EftClient>();

        public static string? GetLiveVersion(Settings settings)
        {
            // Get eft live version
            string eftVersion = FileVersionInfo.GetVersionInfo(Path.Join(settings.LiveEftPath, "EscapeFromTarkov.exe")).ProductVersion?.Replace('-', '.');

            //remove leading 0 from version number
            if (eftVersion != null && eftVersion.StartsWith("0."))
            {
                eftVersion = eftVersion.Remove(0, 2);
            }

            string[] fixedVersion = eftVersion.Split('.')[0..4];

            return string.Join('.', fixedVersion);
        }

        public static EftClient GetClient(string Version)
        {
            return clientList.Where(x => x.Version == Version).FirstOrDefault();
        }

        public static void LoadClientList(Settings settings)
        {
            clientList.Clear();

            string? eftVersion = GetLiveVersion(settings);

            if (eftVersion != null)
            {

                // add eft live version to version options
                clientList.Add(new EftClient()
                {
                    FolderPath = settings.LiveEftPath,
                    Version = eftVersion,
                    PrepPath = Path.Join(settings.PrepFolderPath, eftVersion),
                    Location = EftClientLocation.Live
                });
            }

            // add backup folders to version options
            foreach (string backup in Directory.GetDirectories(settings.BackupFolderPath))
            {
                DirectoryInfo backupDir = new DirectoryInfo(backup);

                if (!backupDir.Exists)
                {
                    continue;
                }

                clientList.Add(new EftClient()
                {
                    FolderPath = backupDir.FullName,
                    Version = backupDir.Name,
                    PrepPath = Path.Join(settings.PrepFolderPath, backupDir.Name),
                    Location = EftClientLocation.Backup
                });
            }
        }

        public static EftClient GetClientSelection(string Prompt)
        {
            SelectionPrompt<EftClient> clientPrompt = new SelectionPrompt<EftClient>()
            {
                Title = Prompt,
                MoreChoicesText = "Move cursor to see more versions",
                PageSize = 10,
                Converter = (x) => x.DisplayName
            };

            clientPrompt.AddChoices(clientList);

            return clientPrompt.Show(AnsiConsole.Console);
        }
    }
}
