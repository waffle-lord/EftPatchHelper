using EftPatchHelper.Helpers;
using EftPatchHelper.Interfaces;
using EftPatchHelper.Model;

namespace EftPatchHelper.Tasks
{
    public class ClientSelectionTask : IClientSelectionTask
    {
        private Settings _settings;
        private Options _options;
        private EftClientSelector _clientSelector;

        public ClientSelectionTask(Settings settings, Options options, EftClientSelector clientSelector)
        {
            _settings = settings;
            _options = options;
            _clientSelector = clientSelector;
        }

        public bool Run()
        {
            
        }
    }
}
