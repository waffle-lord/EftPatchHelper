using Spectre.Console;

namespace EftPatchHelper.Extensions
{
    public static class BoolExtensions
    {
        public static void ValidateOrExit(this bool toValidate)
        {
            if (!toValidate)
            {
                AnsiConsole.Prompt(new TextPrompt<string>("Press [blue]enter[/] to close ...").AllowEmpty());
                Environment.Exit(0);
            }
        }
    }
}
