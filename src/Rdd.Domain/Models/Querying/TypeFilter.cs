using System;
using System.Linq;

namespace Rdd.Domain.Models.Querying
{
    public abstract class TypeFilter<TEntity>
    {
        public abstract IQueryable<TEntity> Apply(IQueryable<TEntity> entities);

        public static TypeFilter<TEntity> FromType(Type subType)
        {
            if (!typeof(TEntity).IsAssignableFrom(subType))
            {
                throw new InvalidOperationException($"Requested sub type {subType.Name} is not valid to create a filter of type {typeof(TypeFilter<TEntity>).Name}");
            }

            return (TypeFilter<TEntity>)typeof(TypeFilter<,>)
                .MakeGenericType(typeof(TEntity), subType)
                .GetConstructors()[0]
                .Invoke(new object[0]);
        }
    }

    public class TypeFilter<TEntity, TSubType> : TypeFilter<TEntity> where TSubType : TEntity
    {
        //used by reflection, do not create another constructor
        public TypeFilter() { }

        public override IQueryable<TEntity> Apply(IQueryable<TEntity> entities)
            => entities.OfType<TSubType>() as IQueryable<TEntity>;
    }
}