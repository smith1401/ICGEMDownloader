using System;
using System.Threading.Tasks;

namespace ICGEMDownloader
{
    class DynamicModel : IModel
    {
        public string Name { get; set; }
        public int Degree { get; set; }
        public string Data { get; set; }
        public string DownLink { get; set; }
        public bool ShouldBeDownloaded { get; set; }
        public bool Downloaded { get; set; }
        public string Date { get; set; }
        public Task Download()
        {
            throw new NotImplementedException();
        }
    }
}
