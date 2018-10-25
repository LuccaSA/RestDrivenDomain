using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Rdd.Infra.Storage
{
    public interface ISaveEventProcessor
    {
        Task<ISavedEntries> InternalBeforeSaveChangesAsync(ChangeTracker changeTracker);
        Task InternalAfterSaveChangesAsync(ISavedEntries savedEntries);
    }
}