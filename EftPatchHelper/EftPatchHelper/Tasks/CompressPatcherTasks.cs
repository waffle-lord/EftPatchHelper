using EftPatchHelper.Extensions;
using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using PizzaOvenApi;
using PizzaOvenApi.Model.PizzaRequests;
using Spectre.Console;

namespace EftPatchHelper.Tasks;

public class CompressPatcherTasks : ICompressPatcherTasks
{
    private Options _options;
    private FileHelper _fileHelper;
    private ZipHelper _zipHelper;
    private PizzaApi _pizzaApi;
    
    public CompressPatcherTasks(Options options, FileHelper fileHelper, ZipHelper zipHelper, PizzaApi pizzaApi)
    {
        _options = options;
        _fileHelper = fileHelper;
        _zipHelper = zipHelper;
        _pizzaApi = pizzaApi;
    }

    public bool CompressPatcher(IProgress<int>? orderProgress)
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

                var progress = new Progress<double>(p =>
                {
                    compressionTask.Increment(p - compressionTask.Percentage);
                    orderProgress?.Report((int)compressionTask.Percentage);
                });

                return _zipHelper.Compress(patchFolder, patchArchiveFile, progress);
            });
    }

    public void Run()
    {
        var order = _pizzaApi.GetCurrentOrder();
        
        var orderProgressReporter = new PizzaOrderProgressHelper(_pizzaApi, 1, "Packing up your order");
        var orderProgress = order != null ? orderProgressReporter.GetProgressReporter(order, PizzaOrderStep.Pack) : null;
        
        CompressPatcher(orderProgress).ValidateOrExit();
    }
}