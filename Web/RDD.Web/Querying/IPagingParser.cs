﻿using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public interface IPagingParser
    {
        QueryPaging ParsePaging();
    }
}