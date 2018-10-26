using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Rdd.Infra.Storage
{
    public sealed class SaveEventProcessor<T> : ISaveEventProcessor where T : class
    {
        private readonly IOnSaveChangesAsync<T> _onSaveChangesHook;

        public SaveEventProcessor(IOnSaveChangesAsync<T> onSaveChangesHook)
        {
            _onSaveChangesHook = onSaveChangesHook;
        }

        public async Task<ISavedEntries> InternalBeforeSaveChangesAsync(ChangeTracker changeTracker)
        {
            IEnumerable<EntityEntry<T>> entityEntries = changeTracker.Entries<T>().ToList();

            var added = entityEntries.Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            var modified = entityEntries.Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToList();
            var deleted = entityEntries.Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();

            var payload = new SavedEntries<T>(added, modified, deleted);

            if (payload.PendingChangesCount == 0)
            {
                // early return : if no pending changed, we don't call OnBeforeSaveAsync / OnAfterSaveAsync
                return payload;
            }

            await _onSaveChangesHook.OnBeforeSaveAsync(payload);
            
            return payload;
        }

        public async Task InternalAfterSaveChangesAsync(ISavedEntries savedEntries)
        {
            await _onSaveChangesHook.OnAfterSaveAsync(savedEntries as SavedEntries<T>);
        }
    }
}