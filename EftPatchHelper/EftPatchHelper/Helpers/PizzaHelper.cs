using System.Text.Json;
using EftPatchHelper.Model;

namespace EftPatchHelper.Helpers;

public class PizzaHelper
{
    private readonly string _apiKey = "";
    private readonly string _apiUrl = "";
    private readonly HttpClient _client;
    
    public PizzaHelper(Settings settings, HttpClient httpClient)
    {
        _client = httpClient;

        if (!settings.UsingPizzaOven())
        {
            return;
        }
        
        _apiKey = settings.PizzaApiKey;
        _apiUrl = settings.PizzaApiUrl;
    }

    public PizzaOrder? PostNewOrder(PizzaOrderData orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        
        var request = PizzaRouteRequest.NewOrder(_apiKey, _apiUrl, json).GetRequest();
        
        var response = _client.Send(request);

        json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var order = JsonSerializer.Deserialize<DataResponse<PizzaOrder>>(json);

        return order?.Data ?? null;
    }

    public PizzaOrder? GetCurrentOrder()
    {
        var request = PizzaRouteRequest.GetCurrentOrder(_apiKey, _apiUrl).GetRequest();

        var response = _client.Send(request);
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var order = JsonSerializer.Deserialize<DataResponse<PizzaOrder>>(json);

        return order?.Data ?? null;
    }

    public bool UpdateOrder(int id, int orderNumber, string message, int progress)
    {
        return UpdateOrder(id, new PizzaOrderData()
        {
            OrderNumber = orderNumber,
            Message = message,
            Progress = progress
        });
    }

    public bool UpdateOrder(int id, PizzaOrderData orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        var request = PizzaRouteRequest.UpdateOrder(_apiKey, _apiUrl, id, json).GetRequest();
        
        var response = _client.Send(request);
        
        return response.IsSuccessStatusCode;
    }
}