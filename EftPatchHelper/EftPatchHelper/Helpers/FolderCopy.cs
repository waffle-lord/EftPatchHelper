using Spectre.Console;

namespace EftPatchHelper.Helpers
{
    public class FolderCopy
    {
        private string SourceFolder { get; }
        private string DestinationFolder { get; }

        /// <summary>
        /// Create a folder copy object
        /// </summary>
        /// <param name="SourceFolder">The folder to copy</param>
        /// <param name="DestinationFolder">The folder to copy into</param>
        public FolderCopy(string SourceFolder, string DestinationFolder)
        {
            this.SourceFolder = SourceFolder;
            this.DestinationFolder = DestinationFolder;
        }

        public bool Start(bool IgnoreIfExists = false, bool merge = false)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(SourceFolder);
            DirectoryInfo destDir = new DirectoryInfo(DestinationFolder);

            if (!destDir.Exists)
            {
                destDir.Create();
                destDir.Refresh();
            }
            else if (IgnoreIfExists)
            {
                AnsiConsole.MarkupLine("[yellow]Exists[/]");
                return true;
            }
            else if(!merge)
            {
                if (!AnsiConsole.Confirm($"{destDir.FullName} exists. Do you want to overwright it?", false))
                {
                    AnsiConsole.MarkupLine("[yellow]Using existing folder[/]");
                    return true;
                }

                destDir.Delete(true);
                destDir.Create();
                destDir.Refresh();
            }

            string[] files = Directory.GetFiles(this.SourceFolder, "*", SearchOption.AllDirectories);

            AnsiConsole.Progress()
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
                var copyTask = ctx.AddTask($"Copying [green]{sourceDir.Name}[/] -> [green]{destDir.Parent?.Name ?? destDir.Name}[/]");

                copyTask.MaxValue = files.Count();

                foreach (string file in files)
                {
                    FileInfo sourceFileInfo = new FileInfo(file);

                    string relativeDestParentPath = sourceFileInfo.DirectoryName?.Replace(sourceDir.FullName, "") ??
                                               throw new Exception($"Failed to get destination file path for {sourceFileInfo.FullName}");

                    DirectoryInfo destParent = new DirectoryInfo(Path.Join(destDir.FullName, relativeDestParentPath));

                    if (!destParent.Exists)
                    {
                        destParent.Create();
                    }

                    string targetFile = Path.Join(destParent.FullName, sourceFileInfo.Name);

                    sourceFileInfo.CopyTo(targetFile, true);

                    copyTask.Increment(1);
                }
            });

            return true;
        }
    }
}
