using EftPatchHelper.EftInfo;
using EftPatchHelper.Model;
using Spectre.Console;
using System.Diagnostics;

namespace EftPatchHelper.Helpers
{
    public class EftClientSelector
    {
        private List<EftClient> _clientList = new List<EftClient>();
        private Settings _settings;

        public EftClientSelector(Settings settings)
        {
            _settings = settings;
        }

        public string? GetLiveVersion()
        {
            // Get eft live version
            string eftVersion = FileVersionInfo.GetVersionInfo(Path.Join(_settings.LiveEftPath, "EscapeFromTarkov.exe")).ProductVersion?.Replace('-', '.');

            //remove leading 0 from version number
            if (eftVersion != null && eftVersion.StartsWith("0."))
            {
                eftVersion = eftVersion.Remove(0, 2);
            }

            string[] fixedVersion = eftVersion.Split('.')[..4];

            return string.Join('.', fixedVersion);
        }

        public EftClient GetClient(string Version)
        {
            return _clientList.Where(x => x.Version == Version).FirstOrDefault();
        }

        public void LoadClientList()
        {
            _clientList.Clear();

            string? eftVersion = GetLiveVersion();

            if (eftVersion != null)
            {

                // add eft live version to version options
                _clientList.Add(new EftClient()
                {
                    FolderPath = _settings.LiveEftPath,
                    Version = eftVersion,
                    PrepPath = Path.Join(_settings.PrepFolderPath, eftVersion),
                    Location = EftClientLocation.Live
                });
            }

            // add backup folders to version options
            foreach (string backup in Directory.GetDirectories(_settings.BackupFolderPath))
            {
                DirectoryInfo backupDir = new DirectoryInfo(backup);

                if (!backupDir.Exists)
                {
                    continue;
                }

                _clientList.Add(new EftClient()
                {
                    FolderPath = backupDir.FullName,
                    Version = backupDir.Name,
                    PrepPath = Path.Join(_settings.PrepFolderPath, backupDir.Name),
                    Location = EftClientLocation.Backup
                });
            }
        }

        public EftClient GetClientSelection(string Prompt, string currentReleaseVersion = "")
        {
            SelectionPrompt<EftClient> clientPrompt = new SelectionPrompt<EftClient>()
            {
                Title = Prompt,
                MoreChoicesText = "Move cursor to see more versions",
                PageSize = 10,
                Converter = (x) =>
                {
                    if (!string.IsNullOrWhiteSpace(currentReleaseVersion) && x.Version.EndsWith(currentReleaseVersion))
                        return $"{x.DisplayName} - Latest Release";

                    return x.DisplayName;
                }
            };

            clientPrompt.AddChoices(_clientList);

            return clientPrompt.Show(AnsiConsole.Console);
        }
    }
}
