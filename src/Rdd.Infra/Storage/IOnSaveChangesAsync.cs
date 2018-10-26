using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    /// <summary>
    /// Interface to implement in order to plug hooks around Before/After SaveChangesAsync()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOnSaveChangesAsync<T> where T : class
    {
        /// <summary>
        /// Called before SaveChangesAsync(), last opportunity to modify entities
        /// </summary>
        Task OnBeforeSaveAsync(SavedEntries<T> savedEntries);

        /// <summary>
        /// Called after SaveChangesAsync(), should be used to apply custom modifications before items are returned via API
        /// </summary>
        Task OnAfterSaveAsync(SavedEntries<T> savedEntries);
    }
}