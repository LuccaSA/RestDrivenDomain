using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;

namespace Rdd.Web.AutoMapper
{
    public static class RddBuilderExtensions
    {
        public static RddBuilder AddAutoMapper(this RddBuilder rddBuilder, Action<IMapperConfigurationExpression> configure)
        {
            rddBuilder.Services.TryAddSingleton(typeof(IRddObjectsMapper<,>), typeof(RddObjectsMapper<,>));

            var mapper = new MapperConfiguration(configure).CreateMapper();
            rddBuilder.Services.TryAddSingleton<IMapper>(mapper);

            return rddBuilder;
        }
    }
}