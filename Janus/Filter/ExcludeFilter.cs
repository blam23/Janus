using System.IO;

namespace Janus.Filter
{
    public class ExcludeFileFilter : Filter
    {
        public ExcludeFileFilter(params string[] filters)
        {
            _filters = filters;
        }

        public override bool ExcludeFile(string fullPath)
        {
            return Matches(fullPath);
        }

        public override bool Matches(string text)
        {
            return base.Matches(Path.GetFileName(text));
        }
    }
}
