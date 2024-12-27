using System.Reflection;
using Spectre.Console;
using System.Security.Cryptography;

namespace EftPatchHelper.Helpers;

public class FileHelper
{
    public string GetFileHash(FileInfo file)
    {
        using (MD5 md5Service = MD5.Create())
        using (var sourceStream = file.OpenRead())
        {
            byte[] sourceHash = md5Service.ComputeHash(sourceStream);

            return Convert.ToBase64String(sourceHash);
        }
    }
    public bool StreamAssemblyResourceOut(string resourceName, string outputFilePath)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            FileInfo outputFile = new FileInfo(outputFilePath);

            if (outputFile.Exists)
            {
                outputFile.Delete();
            }

            if (!outputFile.Directory.Exists)
            {
                Directory.CreateDirectory(outputFile.Directory.FullName);
            }

            var resName = assembly.GetManifestResourceNames().First(x => x.EndsWith(resourceName));

            using (FileStream fs = File.Create(outputFilePath))
            using (Stream s = assembly.GetManifestResourceStream(resName))
            {
                s.CopyTo(fs);
            }

            outputFile.Refresh();
            return outputFile.Exists;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
    }

}