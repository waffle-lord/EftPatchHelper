using System.Text.Json.Serialization;
using Spectre.Console;

namespace EftPatchHelper.Model.PizzaRequests;

public class UpdatePizzaOrderRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("step_progress")]
    public int StepProgress { get; set; }
    
    [JsonPropertyName("current_step")]
    public int CurrentStep { get; set; }

    public UpdatePizzaOrderRequest(string message, int currentStep, int stepProgress)
    {
        Message = message;
        CurrentStep = currentStep;
        StepProgress = stepProgress;
    }
    
    public static UpdatePizzaOrderRequest PromptUpdate(PizzaOrder currentOrder)
    {
        AnsiConsole.MarkupLine($"=== [green] Update order[/] [purple]{currentOrder.OrderNumber}[/] ===");
        var message = new TextPrompt<string>("Enter message: ").Show(AnsiConsole.Console);
        var currentStep = new TextPrompt<int>("Enter current step: ").Show(AnsiConsole.Console);
        var stepProgress = new TextPrompt<int>("Enter progress: ").Show(AnsiConsole.Console);

        return new UpdatePizzaOrderRequest(message, currentStep, stepProgress);
    }
}