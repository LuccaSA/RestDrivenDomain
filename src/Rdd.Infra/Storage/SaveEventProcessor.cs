using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Rdd.Infra.Storage
{
    public abstract class SaveEventProcessor<T> : ISaveEventProcessor where T : class
    {
        public async Task<ISavedEntries> InternalBeforeSaveChangesAsync(ChangeTracker changeTracker)
        {
            IEnumerable<EntityEntry<T>> entityEntries = changeTracker.Entries<T>().ToList();

            var added = entityEntries.Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            var modified = entityEntries.Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToList();
            var deleted = entityEntries.Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();

            var payload = new SavedEntries<T>(added, modified, deleted);

            await OnBeforeSaveAsync(payload);

            return payload;
        }

        public Task InternalAfterSaveChangesAsync(ISavedEntries savedEntries) => OnAfterSaveAsync(savedEntries as SavedEntries<T>);

        /// <summary>
        /// Called before SaveChangesAsync(), last opportunity to modify entities
        /// </summary>
        protected abstract Task OnBeforeSaveAsync(SavedEntries<T> savedEntries);

        /// <summary>
        /// Called after SaveChangesAsync(), should be used to apply custom modifications before items are returned via API
        /// </summary>
        protected abstract Task OnAfterSaveAsync(SavedEntries<T> savedEntries);
    }
}