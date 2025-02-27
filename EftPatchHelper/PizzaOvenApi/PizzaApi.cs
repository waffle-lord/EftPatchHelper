using System.Text.Json;
using PizzaOvenApi.Model;
using PizzaOvenApi.Model.PizzaRequests;

namespace PizzaOvenApi;

public class PizzaApi
{
    public string ApiKey = "";
    public string ApiUrl = "";
    private readonly HttpClient _client;
    
    public PizzaApi(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public PizzaOrder? PostNewOrder(NewPizzaOrderRequest orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        
        var request = PizzaRouteRequest.NewOrder(ApiKey, ApiUrl, json).GetRequest();
        
        var response = _client.Send(request);

        json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var order = JsonSerializer.Deserialize<DataResponse<PizzaOrder>>(json);

        return order?.Data ?? null;
    }

    public PizzaOrder? GetCurrentOrder()
    {
        var request = PizzaRouteRequest.GetCurrentOrder(ApiKey, ApiUrl).GetRequest();

        var response = _client.Send(request);
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        try
        {
            var order = JsonSerializer.Deserialize<DataResponse<PizzaOrder>>(json);
            return order?.Data ?? null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool UpdateOrder(int id, UpdatePizzaOrderRequest orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        var request = PizzaRouteRequest.UpdateOrder(ApiKey, ApiUrl, id, json).GetRequest();
        
        var response = _client.Send(request);
        
        return response.IsSuccessStatusCode;
    }

    public bool CancelOrder(int id)
    {
        var request = PizzaRouteRequest.CancelOrder(ApiKey, ApiUrl, id).GetRequest();

        var response = _client.Send(request);
        
        return response.IsSuccessStatusCode;
    }
}