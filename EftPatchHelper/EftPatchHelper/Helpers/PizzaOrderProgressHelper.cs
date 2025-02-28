using PizzaOvenApi;
using PizzaOvenApi.Model;
using PizzaOvenApi.Model.PizzaRequests;

namespace EftPatchHelper.Helpers;

public class PizzaOrderProgressHelper
{
    private int _partCount;

    private int _currentPart;

    private DateTime _lastUpdate;

    private string _message;
    
    private PizzaApi _pizzaApi;

    public PizzaOrderProgressHelper(PizzaApi pizzaApi, int partCount, string initialMessage = "", int overrideStartingPart = 0)
    {
        if (partCount <= 0)
        {
            throw new ArgumentException("Part count must be greater than 0", nameof(partCount));
        }

        if (overrideStartingPart < -1)
        {
            throw new ArgumentException("Override part count must be greater than or equal to -1", nameof(overrideStartingPart));
        }
        
        _pizzaApi = pizzaApi;
        _partCount = partCount;
        _message = initialMessage;
        _lastUpdate = DateTime.Now;
        _currentPart = overrideStartingPart;
    }

    public IProgress<int> GetProgressReporter(PizzaOrder order, PizzaOrderStep currentStep)
    {
        return new Progress<int>(progress =>
            {
                if (_lastUpdate.AddSeconds(10) >= DateTime.Now)
                {
                    return;
                }

                _lastUpdate = DateTime.Now;

                var partOffset = 100 / _partCount;

                var partProgress = 
                    (double)progress / 100 * ((double)partOffset / 100) * 100 + _currentPart * partOffset;

                var partInfo = _partCount != 1 ? $"{_currentPart + 1} of {_partCount} " : "";
                var message = 
                    $"{order.GetCurrentStepLabel((int)currentStep)}: {partInfo}- {progress}% | {_message}";

                _pizzaApi.UpdateOrder(order.Id, new UpdatePizzaOrderRequest(message, currentStep, (int)partProgress));
            });
    }

    public void IncrementPart(string? newMessage = null)
    {
        _currentPart++;
        _message = newMessage ?? _message;
    }
}