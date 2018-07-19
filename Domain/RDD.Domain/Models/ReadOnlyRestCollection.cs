using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;

namespace RDD.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyRestCollection(IRepository<TEntity> repository, IRightsService rightsService)
        {
            _repository = repository;
            _rightsService = rightsService;
        }

        protected IRightsService _rightsService;
        protected IRepository<TEntity> _repository;

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            query.Options.NeedEnumeration = false;
            query.Options.NeedCount = true;

            return (await GetAsync(query)).Count > 0;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() => (await GetAsync(new Query<TEntity>())).Items;

        public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var count = 0;
            IEnumerable<TEntity> items = new HashSet<TEntity>();

            //Dans de rares cas on veut seulement le count des entités
            if (query.Options.NeedCount && !query.Options.NeedEnumeration)
            {
                count = await _repository.CountAsync(query);
            }
            
            //En général on veut une énumération des entités
            if (query.Options.NeedEnumeration)
            {
                items = await _repository.GetAsync(query);

                count = items.Count();

                //Si y'a plus d'items que le paging max ou que l'offset du paging n'est pas à 0, il faut compter la totalité des entités
                if (query.Page.Offset > 0 || query.Page.Limit <= count)
                {
                    count = await _repository.CountAsync(query);
                }

                query.Page.TotalCount = count;

                //Si on a demandé les permissions, on va les chercher après énumération
                if (query.Options.AttachOperations)
                {
                    AttachOperationsToEntities(items);
                }

                items = await _repository.PrepareAsync(items, query);

                //ON attache les actions après le Prepare, histoire que les objets soient le plus complets possibles
                if (query.Options.AttachActions)
                {
                    AttachActionsToEntities(items);
                }
            }

            //Si c'était un PUT/DELETE, on en profite pour affiner la réponse
            if (query.Verb != HttpVerbs.Get && count == 0)
            {
                throw new NotFoundException(string.Format("No item of type {0} matching URL criteria while trying a {1}", typeof(TEntity).Name, query.Verb));
            }

            return new Selection<TEntity>(items, count);
        }

        /// <summary>
        /// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
        /// puisque c'était explicitement cette entité qui était visée
        /// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            TEntity result = (await GetAsync(new Query<TEntity>(query, e => e.Id.Equals(id)))).Items.FirstOrDefault();

            if (result == null)
            {
                throw new NotFoundException(string.Format("Resource with ID {0} not found", id));
            }

            return result;
        }

        protected void AttachOperationsToEntity(TEntity entity)
        {
            AttachOperationsToEntities(new List<TEntity>
            {
                entity
            });
        }

        private void AttachOperationsToEntities(IEnumerable<TEntity> entities)
        {
            var operationsForAttach = new List<Operation>(); //TODO  _appInstance.GetAllOperations<TEntity>();

            AttachOperations(entities, operationsForAttach);
        }

        /// <summary>
        /// On ne filtre qu'en écriture, pas en lecture
        /// </summary>
        /// <returns></returns>
        protected virtual Query<TEntity> FilterRights(Query<TEntity> query, HttpVerbs verb)
        {
            if (verb == HttpVerbs.Get)
            {
                return query;
            }

            if (!_rightsService.IsAllowed<TEntity>(verb))
            {
                var operations = _rightsService.GetOperationIds<TEntity>(verb);
                if (!operations.Any())
                {
                    throw new UnreachableEntityException(typeof(TEntity));
                }
                throw new UnauthorizedException(string.Format("You do not have sufficient permission to make a {0} on type {1}", verb, typeof(TEntity).Name));
            }

            return query;
        }

        protected virtual void AttachOperations(IEnumerable<TEntity> entities, List<Operation> operations)
        {
            //TODO
            //if (operations.Any())
            //{
            //    var ops = ExecutionContext.Current.curPrincipal.GetOperations(_storage, _appInstance, new HashSet<int>(operations.Select(o => o.Id)));
            //    SetOperationsOnEntities(entities, entities.ToDictionary(o => o.Id, o => ops), operations);
            //}
        }
        
        /// <summary>
        /// Permet d'attacher des actions personnalisées en complément des opérations
        /// </summary>
        /// <param name="list"></param>
        internal virtual void AttachActions(IEnumerable<TEntity> list)
        {
        }

        protected void AttachActionsToEntity(TEntity entity)
        {
            AttachActionsToEntities(new HashSet<TEntity>
            {
                entity
            });
        }

        private void AttachActionsToEntities(IEnumerable<TEntity> list)
        {
            AttachActions(list);
        }

        /// <summary>
        /// When a custom action needs to access the entities operations
        /// </summary>
        /// <param name="entities"></param>
        internal virtual void AppendOperationsToEntities(ICollection<TEntity> entities)
        {
            //TODO
            //var operationsForAttach = _appInstance.GetAllOperations<TEntity>();
            //AttachOperations(entities, operationsForAttach);
        }

        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());

        public async Task<TEntity> TryGetByIdAsync(object id)
        {
            try
            {
                return await GetByIdAsync((TKey) id);
            }
            catch
            {
                return null;
            }
        }

        public Task<TEntity> GetByIdAsync(TKey id, HttpVerbs verb = HttpVerbs.Get) => GetByIdAsync(id, new Query<TEntity>
        {
            Verb = verb
        });
    }
}