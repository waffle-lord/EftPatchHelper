using Spectre.Console;

namespace EftPatchHelper.Extensions
{
    public static class ObjectExtensions
    {
        public static T ValidateOrExit<T>(this object? toValidate)
        {
            if (toValidate == null || toValidate is not T)
            {
                AnsiConsole.Prompt(new TextPrompt<string>("Press [blue]enter[/] to close ...").AllowEmpty());
                Environment.Exit(0);
            }

            return (T)toValidate;
        }
    }
}
