using System.Threading.Tasks;

namespace Janus
{
    public interface ISynchroniser
    {
        SyncData Data { get; set; }

        Task AddAsync(string path, bool isPathFull = true, int retryCount = 5);
        Task DeleteAsync(string path, bool isPathFull = true, int retryCount = 5);
        Task TryFullSynchroniseAsync();
    }
}