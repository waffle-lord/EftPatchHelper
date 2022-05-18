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
        public bool AutoClose { get; set; } = false;

        [JsonPropertyName("patcherExePath")]
        public string PatcherEXEPath { get; set; } = "";

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, typeof(Settings), new JsonSerializerOptions() { WriteIndented = true });

            if (string.IsNullOrWhiteSpace(json)) return;

            File.WriteAllText(settingsFile, json);
        }

        //public static Settings? Load()
        //{
        //    if (!File.Exists(settingsFile)) return null;

        //    string json = File.ReadAllText(settingsFile);

        //    return JsonSerializer.Deserialize<Settings>(json);
        //}

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