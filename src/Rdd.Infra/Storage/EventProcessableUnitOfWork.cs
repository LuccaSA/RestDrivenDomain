using Microsoft.EntityFrameworkCore;
using Rdd.Application;
using Rdd.Domain.Exceptions;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class EventProcessableUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly List<ISaveEventProcessor> _saveEventProcessors;

        public EventProcessableUnitOfWork(DbContext dbContext, IEnumerable<ISaveEventProcessor> saveEventProcessors)
        {
            _dbContext = dbContext;
            _saveEventProcessors = (saveEventProcessors ?? Enumerable.Empty<ISaveEventProcessor>()).ToList();
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                var processed = new List<(ISaveEventProcessor processor, ISavedEntries saved)>(_saveEventProcessors.Count);

                foreach (var processor in _saveEventProcessors)
                {
                    var toSave = await processor.InternalBeforeSaveChangesAsync(_dbContext.ChangeTracker);
                    if (toSave.PendingChangesCount != 0)
                    {
                        processed.Add((processor, toSave));
                    }
                }

                await _dbContext.SaveChangesAsync();

                foreach (var (processor, saved) in processed)
                {
                    await processor.InternalAfterSaveChangesAsync(saved);
                }

            }
            catch (DbUpdateException ex)
            {
                throw (ex.InnerException?.InnerException) switch
                {
                    ArgumentException ae => ae,
                    SqlException se => se.Number switch
                    {
                        2627 => new TechnicalException(se.Message),
                        _ => se,
                    },
                    _ => ex.InnerException ?? ex,
                };
            }
        }
    }
}