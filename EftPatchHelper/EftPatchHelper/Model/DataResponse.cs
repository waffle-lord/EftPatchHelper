using System.Text.Json.Serialization;

namespace EftPatchHelper.Model;

public class DataResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }
}