using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;

namespace EftPatchHelper.Tasks;

public class CompressPatcherTasks : ICompressPatcherTasks
{
    private Options _options;
    private FileHelper _fileHelper;
    private ZipHelper _zipHelper;
    
    public CompressPatcherTasks(Options options, FileHelper fileHelper, ZipHelper zipHelper)
    {
        _options = options;
        _fileHelper = fileHelper;
        _zipHelper = zipHelper;
    }

    public bool CompressPatcher()
    {
        return AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn(Spinner.Known.Dots2)
            })
            .Start(ctx =>
            {

                var compressionTask = ctx.AddTask("Compressing Patcher");
                compressionTask.MaxValue = 100;
                
                if (!_fileHelper.StreamAssemblyResourceOut("7z.dll", _zipHelper.DllPath))
                {
                    return false;
                }

                var patchFolder = new DirectoryInfo(_options.OutputPatchPath);
                
                var patchArchiveFile = new FileInfo(_options.OutputPatchPath + ".7z");

                if (!patchFolder.Exists)
                {
                    return false;
                }

                var progress = new Progress<double>(p => { compressionTask.Increment(p - compressionTask.Percentage);});

                return _zipHelper.Compress(patchFolder, patchArchiveFile, progress);
            });
    }

    public void Run(PizzaOrder? oder = null)
    {
        CompressPatcher().ValidateOrExit();
    }
}