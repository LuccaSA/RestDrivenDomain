using Microsoft.Extensions.Primitives;
using NExtends.Primitives.DateTimes;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        protected IStringConverter StringConverter { get; private set; }
        protected IExpressionParser ExpressionParser { get; private set; }

        public FilterParser(IStringConverter stringConverter, IExpressionParser expressionParser)
        {
            StringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public virtual WebFilter<TEntity> Parse<TEntity>(string key, string value)
        {
            var parts = (value ?? "").Split(',').ToList();
            var operand = ExtractFilterOperand(parts);

            var chain = ExpressionParser.ParseChain<TEntity>(key);
            var values = ConvertFilterValues(operand, chain, parts);

            return new WebFilter<TEntity>(chain, operand, values);
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
                var values = StringConverter.ConvertValues(expression.ResultType, parts);

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