using EftPatchHelper.Model;

namespace EftPatchHelper.Interfaces
{
    public interface ITaskable
    {
        /// <summary>
        /// Runs a predefined task
        /// </summary>
        /// <returns>Returns true if the task succeeded, otherwise false</returns>
        public void Run(PizzaOrder? order);
    }
}
