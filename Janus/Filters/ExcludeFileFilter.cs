using System.Collections.Generic;
using System.IO;
using System.Linq;
using Janus.Matchers;

namespace Janus.Filters
{
    public class ExcludeFileFilter : IFilter
    {
        public FilterBehaviour Behaviour => FilterBehaviour.Blacklist;

        private readonly IPatternMatcher<string> _matcher = new SimpleStringMatcher();

        public IList<string> Filters { get; }

        public ExcludeFileFilter(params string[] filters)
        {
            Filters = filters;
        }

        public ExcludeFileFilter(IList<string> filters)
        {
            Filters = filters;
        }

        public bool ShouldExcludeFile(string fullPath)
        {
            var file = Path.GetFileName(fullPath);
            var ret = false;
            foreach (var filter in Filters)
            {
                if(_matcher.Matches(file, filter))
                {
                    ret = true;
                }
            }
            return ret;
        }

        private bool Equals(ExcludeFileFilter other)
        {
            if (other.Behaviour != Behaviour || other.Filters.Count != Filters.Count) return false;
            return !Filters.Where((filter, i) => filter != other.Filters[i]).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ExcludeFileFilter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_matcher?.GetHashCode() ?? 0)*397) ^ (Filters?.GetHashCode() ?? 0);
            }
        }
    }
}
