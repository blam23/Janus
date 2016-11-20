using Janus.Matchers;
using System.IO;
using System;

namespace Janus.Filters
{
    public class ExcludeFileFilter : IFilter
    {
        public FilterBehaviour Behaviour => FilterBehaviour.Blacklist;

        private IPatternMatcher<string> _matcher = new SimpleStringMatcher();

        private string[] _filters;

        public string[] Filters => _filters;

        public ExcludeFileFilter(params string[] filters)
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
    }
}
