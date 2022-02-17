using System.Text.Json;
using System.Text.Json.Serialization;

namespace EftPatchHelper
{
    public class Settings
    {
        [JsonIgnore]
        public static string settingsFile = Path.Join(Directory.GetCurrentDirectory(), "settings.json");

        [JsonPropertyName("target_eft_version")]
        public string TargetEftVersion { get; set; } = "";

        [JsonPropertyName("prep_folder_path")]
        public string PrepFolderPath { get; set; } = "";

        [JsonPropertyName("backup_folder_path")]
        public string BackupFolderPath { get; set; } = "";

        [JsonPropertyName("live_eft_path")]
        public string LiveEftPath { get; set; } = "";

        [JsonPropertyName("auto_zip")]
        public bool AutoZip { get; set; } = true;

        [JsonPropertyName("patcher_exe_path")]
        public string PatcherEXEPath { get; set; } = "";

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, typeof(Settings), new JsonSerializerOptions() { WriteIndented = true });

            if (string.IsNullOrWhiteSpace(json)) return;

            File.WriteAllText(settingsFile, json);
        }

        public static Settings? Load()
        {
            if (!File.Exists(settingsFile)) return null;

            string json = File.ReadAllText(settingsFile);

            return JsonSerializer.Deserialize<Settings>(json);
        }

        public bool Validate()
        {
            if(string.IsNullOrWhiteSpace(TargetEftVersion)) return false;

            if(string.IsNullOrWhiteSpace(PrepFolderPath)) return false;

            if(string.IsNullOrWhiteSpace(BackupFolderPath)) return false;

            if(string.IsNullOrWhiteSpace(LiveEftPath)) return false;

            if(string.IsNullOrWhiteSpace(PatcherEXEPath)) return false;

            return true;
        }
    }
}