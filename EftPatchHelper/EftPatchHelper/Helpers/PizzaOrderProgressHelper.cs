using EftPatchHelper.Model;
using EftPatchHelper.Model.PizzaRequests;

namespace EftPatchHelper.Helpers;

public class PizzaOrderProgressHelper
{
    private int _partCount;

    private int _currentPart = 0;

    private DateTime _lastUpdate;

    private string _message;
    
    private PizzaHelper _pizzaHelper;

    public PizzaOrderProgressHelper(PizzaHelper pizzaHelper, int partCount = 0, string initialMessage = "")
    {
        _pizzaHelper = pizzaHelper;
        _partCount = partCount;
        _message = initialMessage;
        _lastUpdate = DateTime.Now;
    }

    public IProgress<int> GetProgressReporter(PizzaOrder order, int currentStep)
    {
        return new Progress<int>(progress =>
            {
                if (_lastUpdate.AddSeconds(10) >= DateTime.Now)
                {
                    return;
                }

                _lastUpdate = DateTime.Now;

                var partOffset = 100 / _partCount;

                var partProgress = (double)progress / 100 * ((double)partOffset / 100) * 100 + _currentPart * partOffset;

                _pizzaHelper.UpdateOrder(order.Id, new UpdatePizzaOrderRequest($"{order.GetCurrentStepLabel()}: {_currentPart+1} of {_partCount} - {progress}% | {_message}", currentStep, (int)partProgress));
            });
    }

    public void IncrementPart(string? newMessage = null)
    {
        _currentPart++;
        _message = newMessage ?? _message;
    }
}