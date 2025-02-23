using System.Text.Json.Serialization;
using Spectre.Console;

namespace EftPatchHelper.Model;

public class PizzaOrderData
{
    [JsonPropertyName("order_number")]
    public int OrderNumber { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
    
    [JsonPropertyName("step_progress")]
    public int StepProgress { get; set; } = 0;
    
    [JsonPropertyName("current_step")]
    public int CurrentStep { get; set; } = 0;

    [JsonPropertyName("step_labels")]
    public string StepLabels { get; set; } = "";

    public static PizzaOrderData PromptCreate()
    {
        AnsiConsole.MarkupLine("=== [green] Creating new order[/] ===");
        
        var orderNumber = new TextPrompt<int>("Enter order number: ").Show(AnsiConsole.Console);

        if (orderNumber <= 0)
        {
            throw new ApplicationException("Please enter a valid order number.");
        }
        
        var useBlankOrder = new ConfirmationPrompt("Use blank order template?").Show(AnsiConsole.Console);

        if (useBlankOrder)
        {
            return NewBlankOrder(orderNumber);
        }
        
        var message = new TextPrompt<string>("Enter message: ").DefaultValue("Order Received").Show(AnsiConsole.Console);
        var labels = new TextPrompt<string>("Enter labels: ").Show(AnsiConsole.Console);
        var currentStep = new TextPrompt<int>("Enter current step: ").Show(AnsiConsole.Console);
        var stepProgress = new TextPrompt<int>("Enter progress: ").DefaultValue(0).Show(AnsiConsole.Console);

        return new PizzaOrderData()
        {
            OrderNumber = orderNumber,
            Message = message,
            StepLabels = labels,
            StepProgress = stepProgress,
            CurrentStep = currentStep,
        };
    }

    public static PizzaOrderData PromptUpdate(PizzaOrder currentOrder)
    {
        AnsiConsole.MarkupLine($"=== [green] Update order[/] [purple]{currentOrder.OrderNumber}[/] ===");
        var message = new TextPrompt<string>("Enter message: ").Show(AnsiConsole.Console);
        var currentStep = new TextPrompt<int>("Enter current step: ").Show(AnsiConsole.Console);
        var stepProgress = new TextPrompt<int>("Enter progress: ").Show(AnsiConsole.Console);

        return new PizzaOrderData()
        {
            Message = message,
            CurrentStep = currentStep,
            StepProgress = stepProgress,
        };
    }

    public static PizzaOrderData NewBlankOrder(int orderNumber)
    {
        return new PizzaOrderData
        {
            OrderNumber = orderNumber,
            Message = "New order received! We're getting the kitchen ready!",
            StepLabels = "Setup,Patch,Test,Pack,Upload",
            CurrentStep = 0,
            StepProgress = 0
        };
    }
}