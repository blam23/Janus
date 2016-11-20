using System.IO;

namespace Janus.Filters
{
    public enum FilterBehaviour
    {
        Blacklist,
        Whitelist
    }

    public interface IFilter
    {
        FilterBehaviour Behaviour { get; }
        bool ShouldExcludeFile(string fullPath);
    }
}
