﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using NExtends.Primitives.DateTimes;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Period = Rdd.Domain.Models.Period;

namespace Rdd.Web.Querying
{
    public class FilterParser : IFilterParser
    {
        protected static readonly IReadOnlyDictionary<StringSegment, WebFilterOperand> Operands = new Dictionary<StringSegment, WebFilterOperand>(StringSegmentComparer.OrdinalIgnoreCase)
        {
            {"between", WebFilterOperand.Between},
            {"equals", WebFilterOperand.Equals},
            {"notequal", WebFilterOperand.NotEqual},
            {"like", WebFilterOperand.Like},
            {"since", WebFilterOperand.Since},
            {"starts", WebFilterOperand.Starts},
            {"until", WebFilterOperand.Until},
            {"greaterthan", WebFilterOperand.GreaterThan},
            {"greaterthanorequal", WebFilterOperand.GreaterThanOrEqual},
            {"lessthan", WebFilterOperand.LessThan},
            {"lessthanorequal", WebFilterOperand.LessThanOrEqual}
        };

        private readonly IStringConverter _stringConverter;
        private readonly IExpressionParser _expressionParser;

        public FilterParser(IStringConverter stringConverter, IExpressionParser expressionParser)
        {
            _stringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));
            _expressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public virtual WebFilter<TEntity> Parse<TEntity>(string key, string value)
        {
            var parts = (value ?? "").Split(',').ToList();
            var operand = ExtractFilterOperand(parts);

            var chain = _expressionParser.ParseChain<TEntity>(key);
            var values = ConvertFilterValues(operand, chain, parts);

            return new WebFilter<TEntity>(chain, operand, values);
        }

        public Filter<TEntity> Parse<TEntity>(HttpRequest request, IWebFilterConverter<TEntity> webFilterConverter)
            where TEntity : class
            => Parse(request, null, webFilterConverter);

        public Filter<TEntity> Parse<TEntity>(HttpRequest request, ActionDescriptor action, IWebFilterConverter<TEntity> webFilterConverter)
            where TEntity : class
        {
            var filters = SelectKeys<TEntity>(request, action).Select(kv => Parse<TEntity>(kv.Key, kv.Value));
            return webFilterConverter.ToExpression(filters);
        }

        protected virtual IEnumerable<KeyValuePair<string, StringValues>> SelectKeys<TEntity>(HttpRequest request, ActionDescriptor action)
        {
            var result = request.Query.Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !Reserved.IsKeyword(kv.Key));

            if (action != null)
            {
                var queryActionParameters = action.Parameters
                    .Where(p => p.BindingInfo == null || p.BindingInfo.BindingSource == BindingSource.Query)
                    .Select(p => p.Name)
                    .ToHashSet();

                result = result.Where(kvp => !queryActionParameters.Contains(kvp.Key));
            }
            return result;
        }

        protected virtual WebFilterOperand ExtractFilterOperand(List<string> parts)
        {
            if (parts[0] != null && Operands.ContainsKey(parts[0]))
            {
                var result = Operands[parts[0]];
                parts.RemoveAt(0);
                return result;
            }

            return WebFilterOperand.Equals;
        }

        protected virtual IList ConvertFilterValues(WebFilterOperand operand, IExpression expression, List<string> parts)
        {
            try
            {
                var values = _stringConverter.ConvertValues(expression.ResultType, parts);

                if (operand is WebFilterOperand.Between)
                {
                    if (values.Count == 2 && values[0] is DateTime start && values[1] is DateTime end)
                    {
                        values = new List<Period> { new Period(start, end.ToMidnightTimeIfEmpty()) };
                    }
                    else
                    {
                        throw new BadRequestException("Query parameter is invalid", new FormatException("Correct filter format is 'XXX=between,start,end'."));
                    }
                }

                return values;
            }
            catch (FormatException e)
            {
                throw new BadRequestException("Query parameter is invalid", e);
            }
        }
    }
}