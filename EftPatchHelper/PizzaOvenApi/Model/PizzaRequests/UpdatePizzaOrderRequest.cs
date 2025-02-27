using System.Text.Json.Serialization;

namespace PizzaOvenApi.Model.PizzaRequests;

public class UpdatePizzaOrderRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("step_progress")]
    public int StepProgress { get; set; }
    
    [JsonPropertyName("current_step")]
    public int CurrentStep { get; set; }
    
    public UpdatePizzaOrderRequest(string message, PizzaOrderStep currentStep, int stepProgress)
    {
        Message = message;
        CurrentStep = (int)currentStep;
        StepProgress = stepProgress;
    }
}