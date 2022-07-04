using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EftPatchHelper.Interfaces
{
    public interface IFileUpload
    {
        public string DisplayName { get; set; }
        public string GetLink();
        public Task<bool> UploadAsync(IProgress<double>? progress = null);
    }
}
