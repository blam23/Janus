using System.Collections;
using System.Collections.Generic;

namespace Janus.Filters
{
    public enum FilterBehaviour
    {
        Ignore,
        Include
    }

    public interface IFilter
    {
        FilterBehaviour Behaviour { get; }
        bool ShouldExcludeFile(string fullPath);
        IList<string> Filters { get; }
    }
}
