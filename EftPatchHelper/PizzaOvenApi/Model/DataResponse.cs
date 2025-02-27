using System.Text.Json.Serialization;

namespace PizzaOvenApi.Model;

public class DataResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }
}