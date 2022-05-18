using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EftPatchHelper.Extensions
{
    public static class BoolExtensions
    {
        public static void ValidateOrExit(this bool toValidate)
        {
            if(!toValidate)
            {
                AnsiConsole.Prompt(new TextPrompt<string>("Press [blue]enter[/] to close ...").AllowEmpty());
                Environment.Exit(0);
            }
        }
    }
}
