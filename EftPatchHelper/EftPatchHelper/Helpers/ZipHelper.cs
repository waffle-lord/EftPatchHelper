using SevenZip;
using Spectre.Console;

namespace EftPatchHelper.Helpers;

public class ZipHelper
{
    public string DllPath = Path.Join(Environment.CurrentDirectory, "7z.dll");
    public bool Compress(DirectoryInfo folder, FileInfo outputArchive,
        IProgress<double> progress)
    {
        try
        {
            using var outputStream = outputArchive.OpenWrite();
            
            SevenZipBase.SetLibraryPath(DllPath);

            var compressor = new SevenZipCompressor()
            {
                CompressionLevel = CompressionLevel.Normal,
                CompressionMethod = CompressionMethod.Lzma2,
                ArchiveFormat = OutArchiveFormat.SevenZip
            };

            compressor.Compressing += (_, args) => { progress.Report(args.PercentDone); };
            
            compressor.CompressDirectory(folder.FullName, outputStream);

            outputArchive.Refresh();

            if (!outputArchive.Exists)
            {
                AnsiConsole.MarkupLine("output archive not found");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
    }
}