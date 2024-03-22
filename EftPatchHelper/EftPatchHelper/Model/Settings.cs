using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EftPatchHelper.Model
{
    public class Settings
    {
        [JsonIgnore]
        public static string settingsFile = Path.Join(Directory.GetCurrentDirectory(), "settings.json");

        [JsonPropertyName("targetEftVersion")]
        public string TargetEftVersion { get; set; } = "";

        [JsonPropertyName("prepFolderPath")]
        public string PrepFolderPath { get; set; } = "";

        [JsonPropertyName("backupFolderPath")]
        public string BackupFolderPath { get; set; } = "";

        [JsonPropertyName("liveEftPath")]
        public string LiveEftPath { get; set; } = "";

        [JsonPropertyName("autoZip")]
        public bool AutoZip { get; set; } = true;

        [JsonPropertyName("autoClose")]
        public bool AutoClose { get; set; } = true;

        [JsonPropertyName("patcherExePath")]
        public string PatcherEXEPath { get; set; } = "";

        [JsonPropertyName("giteaApiBasePath")]
        public string GiteaApiBasePath { get; set; } = "";

        [JsonPropertyName("giteaApiKey")]
        public string GiteaApiKey { get; set; } = "";

        [JsonPropertyName("giteaReleaseRepoOwner")]
        public string GiteaReleaseRepoOwner { get; set; } = "";

        [JsonPropertyName("giteaReleaseRepoName")]
        public string GiteaReleaseRepoName { get; set; } = "";

        [JsonPropertyName("megaEmail")]
        public string MegaEmail { get; set; } = "";

        [JsonPropertyName("megaPassword")]
        public string MegaPassword { get; set; } = "";

        [JsonPropertyName("megaUploadFolder")]
        public string MegaUploadFolder { get; set; } = "";

        [JsonPropertyName("goFileApiKey")]
        public string GoFileApiKey { get; set; } = "";

        [JsonPropertyName("goFileFolderId")]
        public string GoFileFolderId { get; set; } = "";

        [JsonPropertyName("sftpUploads")]
        public List<SftpUploadInfo> SftpUploads { get; set; } = new();

        public bool Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, typeof(Settings), new JsonSerializerOptions() { WriteIndented = true });

                if (string.IsNullOrWhiteSpace(json))
                {
                    AnsiConsole.WriteLine("[red]!! Nothing was serialized !![/]");
                    return false;
                }

                File.WriteAllText(settingsFile, json);

                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return false;
            }
        }

        public bool UsingGitea()
        {
            if (string.IsNullOrWhiteSpace(GiteaApiBasePath)) return false;

            if (string.IsNullOrWhiteSpace(GiteaReleaseRepoOwner)) return false;

            if (string.IsNullOrWhiteSpace(GiteaReleaseRepoName)) return false;

            if (string.IsNullOrWhiteSpace(GiteaApiKey)) return false;

            return true;
        }

        public bool UsingMega()
        {
            if (string.IsNullOrWhiteSpace(MegaEmail)) return false;

            if (string.IsNullOrWhiteSpace(MegaPassword)) return false;

            return true;
        }

        public bool UsingGoFile()
        {
            if (string.IsNullOrWhiteSpace(GoFileApiKey)) return false;

            if(string.IsNullOrWhiteSpace(GoFileFolderId)) return false;

            return true;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(TargetEftVersion)) return false;

            if (string.IsNullOrWhiteSpace(PrepFolderPath)) return false;

            if (string.IsNullOrWhiteSpace(BackupFolderPath)) return false;

            if (string.IsNullOrWhiteSpace(LiveEftPath)) return false;

            if (string.IsNullOrWhiteSpace(PatcherEXEPath)) return false;

            return true;
        }
    }
}