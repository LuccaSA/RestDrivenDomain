using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NExtends.Primitives.DateTimes;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using Rdd.Web.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Web.Querying
{
    public class FilterParser<TEntity> : IFilterParser<TEntity>
        where TEntity : class
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
        private readonly IWebFilterConverter<TEntity> _webFilterConverter;
        private readonly IPropertyAuthorizer<TEntity> _propertyAuthorizer;

        public FilterParser(IStringConverter stringConverter, IExpressionParser expressionParser, IWebFilterConverter<TEntity> webFilterConverter, IPropertyAuthorizer<TEntity> propertyAuthorizer)
        {
            _stringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));
            _expressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
            _webFilterConverter = webFilterConverter ?? throw new ArgumentNullException(nameof(webFilterConverter));
            _propertyAuthorizer = propertyAuthorizer ?? throw new ArgumentNullException(nameof(propertyAuthorizer));
        }

        public virtual WebFilter<TEntity> Parse(string key, string value)
        {
            var chain = _expressionParser.ParseChain<TEntity>(key);
            if (!_propertyAuthorizer.IsVisible(chain))
            {
                throw new BadRequestException($"Filter parsing failed for ({key}, {value}).", new ForbiddenException("Selected property is forbidden."));
            }

            var parts = (value ?? "").Split(',').ToList();
            var operand = ExtractFilterOperand(parts);
            var values = ConvertFilterValues(operand, chain, parts);

            return new WebFilter<TEntity>(chain, operand, values);
        }

        public Filter<TEntity> Parse(HttpRequest request) => Parse(request, null);

        public Filter<TEntity> Parse(HttpRequest request, ActionDescriptor action)
        {
            var keyValuePairs = request.Query.Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !Reserved.IsKeyword(kv.Key));

            if (action != null)
            {
                var queryActionParameters = action.Parameters
                    .Where(p => p.BindingInfo == null || p.BindingInfo.BindingSource == BindingSource.Query)
                    .Select(p => p.Name)
                    .ToHashSet();

                keyValuePairs = keyValuePairs.Where(kvp => !queryActionParameters.Contains(kvp.Key));
            }

            var filters = keyValuePairs.Select(kv => Parse(kv.Key, kv.Value));
            return _webFilterConverter.ToExpression(filters);
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