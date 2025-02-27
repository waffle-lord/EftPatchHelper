using System.Text.Json.Serialization;

namespace PizzaOvenApi.Model.PizzaRequests;

public class NewPizzaOrderRequest
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
    
    public static NewPizzaOrderRequest NewBlankOrder(int orderNumber)
    {
        return new NewPizzaOrderRequest
        {
            OrderNumber = orderNumber,
            Message = "New order received! We're getting the kitchen ready!",
            StepLabels = "Setup,Patch,Test,Pack,Upload",
            CurrentStep = 0,
            StepProgress = 0
        };
    }
}