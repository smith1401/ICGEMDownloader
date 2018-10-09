using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICGEMDownloader
{
    public interface IModel
    {
        string Name { get; set; }
        int Degree { get; set; }
        string Data { get; set; }
        string DownLink { get; set; }
        bool ShouldBeDownloaded { get; set; }
        bool Downloaded { get; set; }

        Task Download();
    }
}
