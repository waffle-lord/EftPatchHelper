using System.Text.Json.Serialization;

namespace EftPatchHelper.Model;

public class SftpUploadInfo
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
    
    [JsonPropertyName("hostKey")]
    public string HostKey { get; set; } = "";
    
    [JsonPropertyName("hostname")]
    public string Hostname { get; set; } = "";
    
    [JsonPropertyName("port")]
    public int Port { get; set; } = 0;
    
    [JsonPropertyName("uploadPath")]
    public string UploadPath { get; set; } = "";
    
    [JsonPropertyName("httpPath")]
    public string HttpPath { get; set; } = "";

    public bool Validate()
    {
        if (!string.IsNullOrWhiteSpace(Username))
            return false;
        
        if (!string.IsNullOrWhiteSpace(Password))
            return false;
        
        if (!string.IsNullOrWhiteSpace(HostKey))
            return false;
        
        if (Port == 0)
            return false;
        
        if (!string.IsNullOrWhiteSpace(UploadPath))
            return false;
        
        if (!string.IsNullOrWhiteSpace(HttpPath))
            return false;

        return true;
    }
}