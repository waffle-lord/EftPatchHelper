using Spectre.Console;

namespace EftPatchHelper.Model;

public class NewPizzaOrder
{
    public int OrderNumber { get; set; }
    public string Message { get; set; } = "";
    public int Progress { get; set; } = 0;

    public static NewPizzaOrder PromptCreate()
    {
        var orderNumber = new TextPrompt<int>("Enter order number: ").Show(AnsiConsole.Console);

        if (orderNumber <= 0)
        {
            throw new ApplicationException("Please enter a valid order number.");
        }
        
        var message = new TextPrompt<string>("Enter message: ").DefaultValue("").Show(AnsiConsole.Console);
        var progress = new TextPrompt<int>("Enter progress: ").DefaultValue(0).Show(AnsiConsole.Console);

        return new NewPizzaOrder()
        {
            OrderNumber = orderNumber,
            Message = message,
            Progress = progress
        };
    }
}