using System.Text.Json.Serialization;
using Spectre.Console;

namespace EftPatchHelper.Model;

public class PizzaOrder
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }
    
    [JsonPropertyName("order_number")]
    public int OrderNumber { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
    
    [JsonPropertyName("step_progress")]
    public int StepProgress { get; set; } = 0;
    
    [JsonPropertyName("current_step")]
    public int CurrentStep { get; set; } = 0;

    [JsonPropertyName("step_labels")]
    public string StepLabels { get; set; } = "";
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public string GetCurrentStepLabel()
    {
        var labels = StepLabels.Split(",");

        return CurrentStep >= labels.Length || CurrentStep < 0 ? "--ERROR--" : labels[CurrentStep];
    }

    public void AnsiPrint()
    {
        AnsiConsole.MarkupLine("=== Current Open Order ===");
        AnsiConsole.MarkupLine($"Order #       : [blue]{OrderNumber}[/]");
        AnsiConsole.MarkupLine($"Message       : [blue]{Message.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"Status        : [blue]{Status}[/]");
        AnsiConsole.MarkupLine($"Labels        : [blue]{StepLabels}[/]");
        AnsiConsole.MarkupLine($"Current Step  : [blue]{CurrentStep}[/]");
        AnsiConsole.MarkupLine($"Step Progress : [blue]{StepProgress}[/]");
    }
}