using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Moq;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Expressions.Equality;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Web.AutoMapper;
using Rdd.Web.Controllers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rdd.Web.Tests
{
    public class AutomapperFixture
    {
        public AutomapperFixture()
        {
            Mapper.Initialize(cfg =>
                cfg.AddExpressionMapping()
                .CreateMap<Cat, DTOCat>(MemberList.Destination)
                        .ForMember(dest => dest.NickName, opts => opts.MapFrom(sour => sour.Name))
                        .ForMember(dest => dest.Id, opts => opts.MapFrom(sour => sour.Id))
                        .ForMember(dest => dest.Age, opts => opts.MapFrom(sour => sour.Age))
                    .ReverseMap()
            );
        }
    }

    [CollectionDefinition("automapper")]
    public class AutomapperCollection : ICollectionFixture<AutomapperFixture>
    {
    }

    [Collection("automapper")]
    public class RddObjectsMapperTests
    {
        AutomapperFixture fixture;

        public RddObjectsMapperTests(AutomapperFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void MapperObviousTests()
        {
            Assert.Throws<ArgumentNullException>(() => new RddObjectsMapper<DTOCat, Cat>(null, Mapper.Instance));
            Assert.Throws<ArgumentNullException>(() => new RddObjectsMapper<DTOCat, Cat>(new ExpressionParser(), null));

            var mapper = new RddObjectsMapper<DTOCat, Cat>(new ExpressionParser(), Mapper.Instance);
            Assert.Null(mapper.Map((Query<DTOCat>)null));
            Assert.Null(mapper.Map((ISelection<Cat>)null));
            Assert.Null(mapper.Map((Cat)null));
        }

        [Fact]
        public void MapperTests()
        {
            var mapper = new RddObjectsMapper<DTOCat, Cat>(new ExpressionParser(), Mapper.Instance);
            var cat = new Cat
            {
                Age = 22,
                Name = "bob"
            };

            var dto = mapper.Map(cat);

            Assert.Equal(cat.Age, dto.Age);
            Assert.Equal(cat.Id, dto.Id);
            Assert.Equal(cat.Name, dto.NickName);

            var input = new Selection<Cat>(new List<Cat> { cat }, 45);
            var dtoSelection = mapper.Map(input);

            Assert.Equal(input.Count, dtoSelection.Count);
            Assert.Single(dtoSelection.Items);
            Assert.Equal(cat.Age, dtoSelection.Items.First().Age);
            Assert.Equal(cat.Id, dtoSelection.Items.First().Id);
            Assert.Equal(cat.Name, dtoSelection.Items.First().NickName);

            var expressionParser = new ExpressionParser();
            var query = new Query<DTOCat>
            {
                Filter = new Filter<DTOCat>(f => f.NickName.StartsWith("titi") && f.Age != 10),
                Fields = expressionParser.ParseTree<DTOCat>("nickname,age"),
                OrderBys = new List<OrderBy<DTOCat>> { new OrderBy<DTOCat>(expressionParser.Parse<DTOCat>("nickname").ToLambdaExpression(), SortDirection.Ascending) }
            };
            var bddQuery = mapper.Map(query);

            var comparer = new ExpressionEqualityComparer();
            Expression<Func<Cat, bool>> result = f => f.Name.StartsWith("titi") && f.Age != 10;
            Assert.True(comparer.Equals(bddQuery.Filter, result));

            Assert.Equal(bddQuery.Fields.ToString(), expressionParser.ParseTree<Cat>("name,age").ToString());

            Assert.Single(bddQuery.OrderBys);
            Assert.Equal(SortDirection.Ascending, bddQuery.OrderBys.FirstOrDefault().Direction);
            Expression<Func<Cat, string>> orderBy = f => f.Name;
            Assert.True(comparer.Equals(bddQuery.OrderBys.FirstOrDefault().LambdaExpression, orderBy));
        }

        [Fact]
        public void ControllerObviousTests()
        {
            IReadOnlyAppController<Cat, int> appController = new Mock<IReadOnlyAppController<Cat, int>>().Object;
            IQueryParser<DTOCat> queryParser = new Mock<IQueryParser<DTOCat>>().Object;
            IRddObjectsMapper<DTOCat, Cat> mapper = new Mock<IRddObjectsMapper<DTOCat, Cat>>().Object;

            Assert.Throws<ArgumentNullException>(() => new CatsController(null, queryParser, mapper));
            Assert.Throws<ArgumentNullException>(() => new CatsController(appController, null, mapper));
            Assert.Throws<ArgumentNullException>(() => new CatsController(appController, queryParser, null));
        }
    }
}