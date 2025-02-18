using System.Text.Json;
using EftPatchHelper.Model;

namespace EftPatchHelper.Helpers;

public class PizzaHelper
{
    private string _apiKey = "";
    private string _apiUrl = "";
    private HttpClient _client;
    
    public PizzaHelper(Settings settings, HttpClient httpClient)
    {
        _client = httpClient;
        
        if (settings.UsingPizzaOven())
        {
            _apiKey = settings.PizzaApiKey;
            _apiUrl = settings.PizzaApiUrl;
        }
    }

    private HttpResponseMessage SendApiRequest(HttpMethod method, string url, string json = "")
    {
        var request = new HttpRequestMessage(method, url);
        
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Headers.Add("Accept", "application/json");

        if (!string.IsNullOrWhiteSpace(json))
        {
            request.Headers.Add("Content-Type", "application/json");
            request.Content = new StringContent(json);
        }

        var response = _client.SendAsync(request).GetAwaiter().GetResult();

        return response;
    }

    public bool PostNewOrder(NewPizzaOrder order)
    {
        var json = JsonSerializer.Serialize(order);
        var response = SendApiRequest(HttpMethod.Post, $"{_apiUrl}/api/v1/orders", json);

        return response.IsSuccessStatusCode;
    }

    public PizzaOrder? GetCurrentOrder()
    {
        var response = SendApiRequest(HttpMethod.Get, $"{_apiUrl}/api/v1/orders/current");

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var order = JsonSerializer.Deserialize<DataResponse<PizzaOrder>>(json);

        return order?.Data ?? null;
    }

    public bool UpdateOrder()
    {
        throw new NotImplementedException();
    }
}