using System.Collections.Generic;

namespace Rdd.Infra.Storage
{
    public class SavedEntries<T> : ISavedEntries
        where T : class
    {
        public SavedEntries(IEnumerable<T> added, IEnumerable<T> modified, IEnumerable<T> deleted)
        {
            Added = added;
            Modified = modified;
            Deleted = deleted;
        }

        public IEnumerable<T> Added { get; }
        public IEnumerable<T> Modified { get; }
        public IEnumerable<T> Deleted { get; }
    }

    public interface ISavedEntries
    {
        // empty class to avoid passing object
    }
}