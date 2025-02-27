using System.Text.Json.Serialization;

namespace PizzaOvenApi.Model;

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

    public string GetCurrentStepLabel(int currentStep)
    {
        var labels = StepLabels.Split(",");

        return currentStep >= labels.Length || currentStep < 0 ? "--ERROR--" : labels[currentStep];
    }
}