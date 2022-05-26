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