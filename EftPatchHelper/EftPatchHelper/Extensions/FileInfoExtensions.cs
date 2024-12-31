namespace EftPatchHelper.Extensions;

public static class FileInfoExtensions
{
    public static string HumanLength(this FileInfo fileInfo)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            
        if (fileInfo.Length == 0)
        {
            return "0" + suf[0];
        }
            
        long bytes = Math.Abs(fileInfo.Length);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(fileInfo.Length) * num) + suf[place];
    }
}