using AutoMapper;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Querying;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Web.AutoMapper
{
    public class RddObjectsMapper<TEntityDto, TEntity> : IRddObjectsMapper<TEntityDto, TEntity>
        where TEntityDto : class
        where TEntity : class
    {
        private readonly IExpressionParser _expressionParser;
        private readonly IMapper _mapper;

        public RddObjectsMapper(IExpressionParser expressionParser, IMapper mapper)
        {
            _expressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public TEntityDto Map(TEntity source) => _mapper.Map<TEntityDto>(source);

        public ISelection<TEntityDto> Map(ISelection<TEntity> source)
        {
            if (source == null)
            {
                return null;
            }
            return new Selection<TEntityDto>(source.Items.Select(_mapper.Map<TEntityDto>), source.Count);
        }

        public Query<TEntity> Map(Query<TEntityDto> query)
        {
            if (query == null)
            {
                return null;
            }

            return new Query<TEntity>(MapFields(query.Fields), query.OrderBys.Select(MapOrderBy).ToList(), query.Page, MapFilters(query.Filter), query.Verb);
        }

        protected IExpressionTree<TEntity> MapFields(IExpressionTree<TEntityDto> fields)
            => _expressionParser.ParseTree<TEntity>(fields.Select(c => MapLambda(c.ToLambdaExpression())).ToArray());

        protected OrderBy<TEntity> MapOrderBy(OrderBy<TEntityDto> orderBy)
            => new OrderBy<TEntity>(MapLambda(orderBy.LambdaExpression), orderBy.Direction);

        protected Expression<Func<TEntity, bool>> MapFilters(Expression<Func<TEntityDto, bool>> filter)
            => _mapper.Map<Expression<Func<TEntity, bool>>>(filter);

        protected LambdaExpression MapLambda(LambdaExpression source)
        {
            var destinationType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(typeof(TEntity), source.ReturnType));
            var result = _mapper.Map(source, source.GetType(), destinationType);

            return result as LambdaExpression;
        }
    }
}