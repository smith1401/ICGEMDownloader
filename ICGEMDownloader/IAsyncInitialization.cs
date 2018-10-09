using System.Threading.Tasks;

namespace ICGEMDownloader
{
    interface IAsyncInitialization
    {
        Task Initialization { get; }
    }
}
