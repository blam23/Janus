namespace Janus
{
    public interface ISynchroniser
    {
        SyncData Data { get; set; }

        void AddAsync(string path, bool isPathFull = true, int retryCount = 5);
        void DeleteAsync(string path, bool isPathFull = true, int retryCount = 5);
        void TryFullSynchronise();
    }
}