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

        [JsonPropertyName("latestReleaseUrl")]
        public string LatestReleaseUrl { get; set; } = "";

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

        [JsonPropertyName("r2ConnectedDomainUrl")]
        public string R2ConnectedDomainUrl { get; set; } = "";

        [JsonPropertyName("r2ServiceUrl")]
        public string R2ServiceUrl { get; set; } = "";

        [JsonPropertyName("r2BucketName")]
        public string R2BucketName { get; set; } = "";
        
        [JsonPropertyName("r2AccessKeyId")]
        public string R2AccessKeyId { get; set; } = "";
        
        [JsonPropertyName("r2SecretKeyId")]
        public string R2SecretKeyId { get; set; } = "";

        [JsonPropertyName("sftpUploads")]
        public List<SftpUploadInfo> SftpUploads { get; set; } = new();
        
        [JsonPropertyName("pizzaApiUrl")]
        public string PizzaApiUrl { get; set; } = "";
        
        [JsonPropertyName("pizzaApiKey")]
        public string PizzaApiKey { get; set; } = "";

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

        public bool UsingMega()
        {
            return !string.IsNullOrWhiteSpace(MegaEmail) && !string.IsNullOrWhiteSpace(MegaPassword);
        }

        public bool UsingGoFile()
        {
            return !string.IsNullOrWhiteSpace(GoFileApiKey) && !string.IsNullOrWhiteSpace(GoFileFolderId);
        }

        public bool UsingR2()
        {
            return !string.IsNullOrWhiteSpace(R2ConnectedDomainUrl)
                   && !string.IsNullOrWhiteSpace(R2ServiceUrl)
                   && !string.IsNullOrWhiteSpace(R2BucketName)
                   && !string.IsNullOrWhiteSpace(R2AccessKeyId)
                   && !string.IsNullOrWhiteSpace(R2SecretKeyId);
        }

        public bool UsingPizzaOven()
        {
            return !string.IsNullOrWhiteSpace(PizzaApiKey) && !string.IsNullOrWhiteSpace(PizzaApiUrl);
        }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(TargetEftVersion)
                   && !string.IsNullOrWhiteSpace(PrepFolderPath)
                   && !string.IsNullOrWhiteSpace(BackupFolderPath)
                   && !string.IsNullOrWhiteSpace(LiveEftPath)
                   && !string.IsNullOrWhiteSpace(PatcherEXEPath);
        }
    }
}