using System.Collections.Generic;
using Janus.Filters;

namespace Janus
{
    public class SyncData
    {
        protected bool Equals(SyncData other)
        {
            return AddFiles == other.AddFiles && 
                DeleteFiles == other.DeleteFiles && Recursive == other.Recursive && 
                string.Equals(WatchDirectory, other.WatchDirectory) && 
                string.Equals(SyncDirectory, other.SyncDirectory);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SyncData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Filters?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ AddFiles.GetHashCode();
                hashCode = (hashCode * 397) ^ DeleteFiles.GetHashCode();
                hashCode = (hashCode * 397) ^ Recursive.GetHashCode();
                hashCode = (hashCode * 397) ^ (WatchDirectory?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SyncDirectory?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        // TODO: Implement saving these on change
        public List<IFilter> Filters = new List<IFilter>(); // todo: to prop?
        public bool AddFiles { get; set; }
        public bool DeleteFiles { get; set; }
        public bool Recursive { get; set; }
        public string WatchDirectory { get; set; }
        public string SyncDirectory { get; set; }
    }
}
