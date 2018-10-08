﻿using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using System;

namespace Rdd.Web.Serialization.Serializers
{
    public class DateTimeSerializer : ValueSerializer
    {
        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            switch (entity)
            {
                case DateTime d:
                    writer.WriteValue(DateTime.SpecifyKind(d, DateTimeKind.Unspecified));
                    break;

                default:
                    base.WriteJson(writer, entity, fields);
                    break;
            }
        }
    }
}