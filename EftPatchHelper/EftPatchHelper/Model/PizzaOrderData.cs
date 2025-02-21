using System.Text.Json.Serialization;
using Spectre.Console;

namespace EftPatchHelper.Model;

public class PizzaOrderData
{
    [JsonPropertyName("order_number")]
    public int OrderNumber { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("progress")]
    public int Progress { get; set; } = 0;

    public static PizzaOrderData PromptCreate()
    {
        AnsiConsole.MarkupLine("=== [green] Creating new order[/] ===");
        var orderNumber = new TextPrompt<int>("Enter order number: ").Show(AnsiConsole.Console);

        if (orderNumber <= 0)
        {
            throw new ApplicationException("Please enter a valid order number.");
        }
        
        var message = new TextPrompt<string>("Enter message: ").DefaultValue("Order Received").Show(AnsiConsole.Console);
        var progress = new TextPrompt<int>("Enter progress: ").DefaultValue(0).Show(AnsiConsole.Console);

        return new PizzaOrderData()
        {
            OrderNumber = orderNumber,
            Message = message,
            Progress = progress
        };
    }

    public static PizzaOrderData PromptUpdate(PizzaOrder currentOrder)
    {
        AnsiConsole.MarkupLine($"=== [green] Update order[/] [purple]{currentOrder.OrderNumber}[/] ===");
        var message = new TextPrompt<string>("Enter message: ").Show(AnsiConsole.Console);
        var progress = new TextPrompt<int>("Enter progress: ").Show(AnsiConsole.Console);

        return new PizzaOrderData()
        {
            OrderNumber = currentOrder.OrderNumber,
            Message = message,
            Progress = progress
        };
    }
}