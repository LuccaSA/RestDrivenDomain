using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Helpers;

namespace Rdd.Z.EntityFramework
{
    public static class ZEntityExtensions
    {
        private const string OptimizeIncludes = "OptimizeIncludes";

        public static RddBuilder AddZEntityOptimizeInclude(this RddBuilder rddBuilder)
        {
            rddBuilder.Services.AddSingleton<IIncludeApplicator, ZEntityIncludeApplicator>();
            return rddBuilder;
        }

        /// <summary>
        /// Warning : use only in case of multiple includes, and by testing the behavior before and after enabling this.
        /// This property can lead to under-perform in some cases, so use it with caution
        /// https://entityframework-plus.net/query-include-optimized
        /// </summary>
        public static void ForceOptimizeIncludes<TEntity>(this Query<TEntity> query, bool optimize)
            where TEntity : class
        {
            if (query.Options.CustomOptions == null)
            {
                query.Options.CustomOptions = new Dictionary<string, object> {{OptimizeIncludes, true}};
            }
            else
            {
                query.Options.CustomOptions.Add(OptimizeIncludes, true);
            }
        }

        internal static bool GetOptimizeIncludes<TEntity>(this Query<TEntity> query)
            where TEntity : class
        {
            if (query.Options.CustomOptions != null
                && query.Options.CustomOptions.TryGetValue(OptimizeIncludes, out object found)
                && found is bool val)
            {
                return val;
            }
            return false;
        }
    }
}
