using System.Text.Json.Serialization;

namespace EftPatchHelper.Model
{
    public class DownloadMirror
    {
        [JsonIgnore] 
        public bool AddHubEntry { get; set; }
        public string Link { get; set; }
        public string Hash { get; set; }
    }
}
