using PizzaOvenApi.Model;
using PizzaOvenApi.Model.PizzaRequests;
using Spectre.Console;

namespace EftPatchHelper.Helpers;

public static class PizzaHelper
{
    public static void AnsiPrint(PizzaOrder order)
    {
        AnsiConsole.MarkupLine("=== Current Open Order ===");
        AnsiConsole.MarkupLine($"Order #       : [blue]{order.OrderNumber}[/]");
        AnsiConsole.MarkupLine($"Message       : [blue]{order.Message.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"Status        : [blue]{order.Status}[/]");
        AnsiConsole.MarkupLine($"Labels        : [blue]{order.StepLabels}[/]");
        AnsiConsole.MarkupLine($"Current Step  : [blue]{order.CurrentStep}[/]");
        AnsiConsole.MarkupLine($"Step Progress : [blue]{order.StepProgress}[/]");
    }
    
    public static NewPizzaOrderRequest PromptCreate()
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
            return NewPizzaOrderRequest.NewBlankOrder(orderNumber);
        }
        
        var message = new TextPrompt<string>("Enter message: ").DefaultValue("Order Received").Show(AnsiConsole.Console);
        var labels = new TextPrompt<string>("Enter labels: ").Show(AnsiConsole.Console);
        var currentStep = new TextPrompt<int>("Enter current step: ").Show(AnsiConsole.Console);
        var stepProgress = new TextPrompt<int>("Enter progress: ").DefaultValue(0).Show(AnsiConsole.Console);

        return new NewPizzaOrderRequest()
        {
            OrderNumber = orderNumber,
            Message = message,
            StepLabels = labels,
            StepProgress = stepProgress,
            CurrentStep = currentStep,
        };
    }
    
    public static UpdatePizzaOrderRequest PromptUpdate(PizzaOrder currentOrder)
    {
        AnsiConsole.MarkupLine($"=== [green] Update order[/] [purple]{currentOrder.OrderNumber}[/] ===");
        var message = new TextPrompt<string>("Enter message: ").Show(AnsiConsole.Console);
        
        var currentStep = new TextPrompt<PizzaOrderStep>("Enter current step: ").Show(AnsiConsole.Console);
        
        
        var stepProgress = new TextPrompt<int>("Enter progress: ").Show(AnsiConsole.Console);

        return new UpdatePizzaOrderRequest(message, currentStep, stepProgress);
    }
}