using Janus.Matchers;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Filters
{
    public class ExcludeFileFilter : IFilter
    {
        public FilterBehaviour Behaviour => FilterBehaviour.Blacklist;

        private readonly IPatternMatcher<string> _matcher = new SimpleStringMatcher();

        private IList<string> _filters;

        public IList<string> Filters => _filters;

        public ExcludeFileFilter(params string[] filters)
        {
            _filters = filters;
        }

        public ExcludeFileFilter(IList<string> filters)
        {
            _filters = filters;
        }

        public bool ShouldExcludeFile(string fullPath)
        {
            var file = Path.GetFileName(fullPath);
            var ret = false;
            foreach (var filter in _filters)
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
                return ((_matcher?.GetHashCode() ?? 0)*397) ^ (_filters?.GetHashCode() ?? 0);
            }
        }
    }
}
