using System.Text;

namespace PizzaOvenApi.Model;

public class PizzaRouteRequest
{
    private HttpMethod _httpMethod;
    private string _apiKey;
    private string _url;
    private string _json;
    
    private PizzaRouteRequest(HttpMethod method, string apiKey, string url, string json = "")
    {
        _httpMethod = method;
        _apiKey = apiKey;
        _url = url;
        _json = json;
    }

    private static string CombineUrl(string url, string endpoint)
    {
        // both have slashes
        if (url.EndsWith("/") && endpoint.StartsWith("/"))
        {
            return url + endpoint[1..];
        }
        
        // both don't have slashes
        if (!url.EndsWith("/") && !endpoint.StartsWith("/"))
        {
            return url + "/" + endpoint;
        }

        // only one has a slash
        return url + endpoint;
    }

    public static PizzaRouteRequest GetCurrentOrder(string apiKey, string url)
    {
        url = CombineUrl(url, "orders/current");
        return new PizzaRouteRequest(HttpMethod.Get, apiKey, url);
    }
    
    public static PizzaRouteRequest NewOrder(string apiKey, string url, string json)
    {
        url = CombineUrl(url, "orders");
        return new PizzaRouteRequest(HttpMethod.Post, apiKey, url, json);
    }

    public static PizzaRouteRequest UpdateOrder(string apiKey, string url, int id, string json)
    {
        url = CombineUrl(url, $"orders/{id}");
        return new PizzaRouteRequest(HttpMethod.Patch, apiKey, url, json);
    }

    public static PizzaRouteRequest CancelOrder(string apiKey, string url, int id)
    {
        url = CombineUrl(url, $"orders/cancel/{id}");
        return new PizzaRouteRequest(HttpMethod.Put, apiKey, url);
    }

    public HttpRequestMessage GetRequest()
    {
        var request = new HttpRequestMessage(_httpMethod, _url);
        
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Headers.Add("Accept", "application/json");

        if (string.IsNullOrWhiteSpace(_json))
        {
            return request;
        }

        request.Content = new StringContent(_json, Encoding.UTF8, "application/json");
        
        return request;
    }
}