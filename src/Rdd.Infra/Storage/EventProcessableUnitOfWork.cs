using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rdd.Application;
using Rdd.Infra.Exceptions;

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

                foreach (var savedEvent in processed)
                {
                    await savedEvent.processor.InternalAfterSaveChangesAsync(savedEvent.saved);
                }

            }
            catch (DbUpdateException ex)
            {
                switch (ex.InnerException?.InnerException)
                {
                    case ArgumentException ae:
                        throw ae;
                    case SqlException se:
                        switch (se.Number)
                        {
                            case 2627:
                                throw new SqlUniqConstraintException(se.Message);
                            default:
                                throw se;
                        }
                    default:
                        throw ex.InnerException ?? ex;
                }
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}