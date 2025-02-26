using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;
using Spectre.Console;
using System.Text.Json;
using EftPatchHelper.Helpers;
using EftPatchHelper.Model.PizzaRequests;

namespace EftPatchHelper.Tasks
{
    public class UploadMirrorListTasks : IMirrorUploader
    {
        private Settings _settings;
        private Options _options;
        private R2Helper _r2;
        private PizzaHelper _pizzaHelper;

        public UploadMirrorListTasks(Settings settigns, Options options, R2Helper r2, PizzaHelper pizzaHelper)
        {
            _settings = settigns;
            _options = options;
            _r2 = r2;
            _pizzaHelper = pizzaHelper;
        }

        private async Task<bool> UploadMirrorList(FileInfo file)
        {
            return await AnsiConsole.Progress().Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn() { Alignment = Justify.Left },
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(Spinner.Known.Dots2),
                })
                .StartAsync(async ctx =>
                {
                    var uploadTask = ctx.AddTask("mirrors.json upload");

                    var progress = new Progress<double>((p) => { uploadTask.Value = p; });

                    return await _r2.UploadToBucketAsync(file, progress);
                });
        }

        public bool CreateMirrorList(FileInfo mirrorListFileInfo)
        {
            var sourcePatchVersion = _options.SourceClient.Version.Split('.').Last();
            var targetPatchVersion = _options.TargetClient.Version.Split('.').Last();
            var mirrorInfo = new PatchInfo
            {
                SourceClientVersion = int.Parse(sourcePatchVersion),
                TargetClientVersion = int.Parse(targetPatchVersion),
                Mirrors = _options.MirrorList.Values.ToList() 
            };

            string json = JsonSerializer.Serialize(mirrorInfo, new JsonSerializerOptions() { WriteIndented = true });

            File.WriteAllText(mirrorListFileInfo.FullName, json);

            mirrorListFileInfo.Refresh();

            return mirrorListFileInfo.Exists;
        }

        public void Run()
        {
            if (!_settings.UsingR2() || !_options.UplaodToR2)
            {
                return;
            }
            
            AnsiConsole.WriteLine();

            var fileInfo = new FileInfo(Path.Join(Environment.CurrentDirectory, "mirrors.json"));

            CreateMirrorList(fileInfo);

            var success = UploadMirrorList(fileInfo).GetAwaiter().GetResult();
            
            var order = _pizzaHelper.GetCurrentOrder();

            if (order == null)
            {
                return;
            }

            if (!success)
            {
                _pizzaHelper.CancelOrder(order.Id);
                return;
            }

            var request = new UpdatePizzaOrderRequest("Order Complete", PizzaOrderStep.Upload, 100);
            
            _pizzaHelper.UpdateOrder(order.Id, request);
            AnsiConsole.WriteLine($"Order #{order.OrderNumber} Completed");
        }
    }
}
