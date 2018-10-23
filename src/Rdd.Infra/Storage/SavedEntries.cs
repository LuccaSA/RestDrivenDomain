using System.Collections.Generic;

namespace Rdd.Infra.Storage
{
    public class SavedEntries<T> : ISavedEntries
        where T : class
    {
        public SavedEntries(List<T> added, List<T> modified, List<T> deleted)
        {
            Added = added;
            Modified = modified;
            Deleted = deleted;
            PendingChangesCount = added.Count + modified.Count + deleted.Count;
        }

        public IEnumerable<T> Added { get; }
        public IEnumerable<T> Modified { get; }
        public IEnumerable<T> Deleted { get; }
        public int PendingChangesCount { get; }
    }

    public interface ISavedEntries
    {
        int PendingChangesCount { get; }
    }
}