using EftPatchHelper.Model;

namespace EftPatchHelper.Helpers;

public class PizzaHelper
{
    private string _apiKey = "";
    private HttpClient _client;
    
    public PizzaHelper(Settings settings, HttpClient httpClient)
    {
        _client = httpClient;
        
        if (settings.UsingPizzaOven())
        {
            _apiKey = settings.PizzaApiKey;
        }
    }

    public bool PostNewOrder()
    {
        throw new NotImplementedException();
    }

    public PizzaOrder GetCurrentOrder()
    {
        throw new NotImplementedException();
    }

    public bool UpdateOrder()
    {
        throw new NotImplementedException();
    }
}