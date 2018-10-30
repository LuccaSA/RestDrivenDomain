﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IQueryParser<TEntity> where TEntity : class
    {
        Query<TEntity> Parse(HttpRequest request, bool isCollectionCall);
        Query<TEntity> Parse(HttpRequest request, ActionDescriptor action, bool isCollectionCall);
    }
}