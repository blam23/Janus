using System.Collections.Generic;
using System.Linq;
using Janus.Matchers;

namespace Janus.Filters
{
    public class IncludeFilter : IFilter
    {
        public FilterBehaviour Behaviour => FilterBehaviour.Include;

        private readonly IPatternMatcher<string> _matcher = new SimpleStringMatcher();

        private readonly IList<string> _filters;

        public IList<string> Filters => _filters;

        public IncludeFilter(params string[] filters)
        {
            _filters = filters;
        }

        public IncludeFilter(IList<string> filters)
        {
            _filters = filters;
        }
        public bool ShouldExcludeFile(string fullPath)
        {
            var ret = true;
            foreach (var filter in _filters)
            {
                if (_matcher.Matches(fullPath, filter))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private bool Equals(IncludeFilter other)
        {
            if (other.Behaviour != Behaviour || other.Filters.Count != Filters.Count) return false;
            return !Filters.Where((filter, i) => filter != other.Filters[i]).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IncludeFilter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_matcher?.GetHashCode() ?? 0) * 397) ^ (_filters?.GetHashCode() ?? 0);
            }
        }
    }
}
