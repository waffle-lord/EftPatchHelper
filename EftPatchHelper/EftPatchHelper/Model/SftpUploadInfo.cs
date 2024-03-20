namespace EftPatchHelper.Model;

public class SftpUploadInfo
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string HostKey { get; set; } = "";
    public string Hostname { get; set; } = "";
    public int Port { get; set; } = 0;
    public string uploadPath { get; set; } = "";
    public string HttpPath { get; set; } = "";
}