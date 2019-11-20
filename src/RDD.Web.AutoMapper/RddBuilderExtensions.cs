using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;

namespace Rdd.Web.AutoMapper
{
    public static class RddBuilderExtensions
    {
        public static RddBuilder AddAutoMapper(this RddBuilder rddBuilder)
        {
            rddBuilder.Services.TryAddSingleton(typeof(IRddObjectsMapper<,>), typeof(RddObjectsMapper<,>));

            return rddBuilder;
        }
    }
}