using System.Text.Json;
using PizzaOvenApi.Model;
using PizzaOvenApi.Model.PizzaRequests;

namespace PizzaOvenApi;

public class PizzaApi
{
    private readonly string _apiKey = "";
    private readonly string _apiUrl = "";
    private readonly HttpClient _client;
    
    public PizzaApi(string apiKey, string apiUrl, HttpClient httpClient)
    {
        _apiKey = apiKey;
        _apiUrl = apiUrl;
        _client = httpClient;
    }
    
    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="orderData">The new order request data to use</param>
    /// <returns>The newly create order or null</returns>
    public PizzaOrder? PostNewOrder(NewPizzaOrderRequest orderData) => PostNewOrderAsync(orderData).GetAwaiter().GetResult();

    /// <summary>
    /// Create a new order async
    /// </summary>
    /// <param name="orderData">The new order request data to use</param>
    /// <returns>The newly create order or null</returns>
    public async Task<PizzaOrder?> PostNewOrderAsync(NewPizzaOrderRequest orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        
        var request = PizzaRouteRequest.NewOrder(_apiKey, _apiUrl, json).GetRequest();
        
        var response = await _client.SendAsync(request);

        json = await response.Content.ReadAsStringAsync();

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

    /// <summary>
    /// Get the current order
    /// </summary>
    /// <returns>The current order, or null if one doesn't exist</returns>
    public PizzaOrder? GetCurrentOrder() => GetCurrentOrderAsync().GetAwaiter().GetResult();
    
    /// <summary>
    /// Get the current order async
    /// </summary>
    /// <returns>The current order, or null if one doesn't exist</returns>
    public async Task<PizzaOrder?> GetCurrentOrderAsync()
    {
        var request = PizzaRouteRequest.GetCurrentOrder(_apiKey, _apiUrl).GetRequest();

        var response = await _client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

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

    /// <summary>
    /// Get the last completed order
    /// </summary>
    /// <returns>The last completed order, or null</returns>
    public PizzaOrder? GetLastCompletedOrder() => GetLastCompletedOrderAsync().GetAwaiter().GetResult();
    
    /// <summary>
    /// Get the last completed order async
    /// </summary>
    /// <returns>The last completed order, or null</returns>
    public async Task<PizzaOrder?> GetLastCompletedOrderAsync()
    {
        var request = PizzaRouteRequest.GetLastCompletedOrder(_apiKey, _apiUrl).GetRequest();
        
        var response = await _client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

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

    /// <summary>
    /// Update an existing order
    /// </summary>
    /// <param name="id">The ID of the order to update</param>
    /// <param name="orderData">The update order request data to use</param>
    /// <returns>True if the order was updated, otherwise false</returns>
    public bool UpdateOrder(int id, UpdatePizzaOrderRequest orderData) => UpdateOrderAsync(id, orderData).GetAwaiter().GetResult();
    
    /// <summary>
    /// Update an existing order async
    /// </summary>
    /// <param name="id">The ID of the order to update</param>
    /// <param name="orderData">The update order request data to use</param>
    /// <returns>True if the order was updated, otherwise false</returns>
    public async Task<bool> UpdateOrderAsync(int id, UpdatePizzaOrderRequest orderData)
    {
        var json = JsonSerializer.Serialize(orderData);
        var request = PizzaRouteRequest.UpdateOrder(_apiKey, _apiUrl, id, json).GetRequest();
        
        var response = await _client.SendAsync(request);
        
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Cancel an existing order
    /// </summary>
    /// <param name="id">The ID of the order to cancel</param>
    /// <returns>True if the order was cancelled, otherwise false</returns>
    public bool CancelOrder(int id) => CancelOrderAsync(id).GetAwaiter().GetResult();
    
    /// <summary>
    /// Cancel an existing order async
    /// </summary>
    /// <param name="id">The ID of the order to cancel</param>
    /// <returns>True if the order was cancelled, otherwise false</returns>
    public async Task<bool> CancelOrderAsync(int id)
    {
        var request = PizzaRouteRequest.CancelOrder(_apiKey, _apiUrl, id).GetRequest();

        var response = await _client.SendAsync(request);
        
        return response.IsSuccessStatusCode;
    }
}