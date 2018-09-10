using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    /// <summary>
    /// source : http://stackoverflow.com/questions/283537/most-efficient-way-to-test-equality-of-lambda-expressions
    /// </summary>
    public static class ExpressionEqualityComparer
    {
        public static bool Eq(LambdaExpression x, LambdaExpression y) => ExpressionsEqual(x, y, null, null);

        public static bool Eq<TSource1, TSource2, TValue>(Expression<Func<TSource1, TSource2, TValue>> x, Expression<Func<TSource1, TSource2, TValue>> y)
            => ExpressionsEqual(x, y, null, null);

        public static Expression<Func<Expression<Func<TSource, TValue>>, bool>> Eq<TSource, TValue>(Expression<Func<TSource, TValue>> y)
            => x => ExpressionsEqual(x, y, null, null);

        private static bool ExpressionsEqual(Expression x, Expression y, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            var valueX = TryCalculateConstant(x);
            var valueY = TryCalculateConstant(y);

            if (valueX.IsDefined && valueY.IsDefined)
            {
                return ValuesEqual(valueX.Value, valueY.Value);
            }

            if (x.NodeType != y.NodeType || !(y.Type.IsAssignableFrom(x.Type) || x.Type.IsAssignableFrom(y.Type)))
            {
                if (IsAnonymousType(x.Type) && IsAnonymousType(y.Type))
                {
                    throw new NotImplementedException("Comparison of Anonymous Types is not supported");
                }

                return false;
            }

            switch (x)
            {
                case LambdaExpression lx:
                    var ly = (LambdaExpression)y;
                    return CollectionsEqual(lx.Parameters, ly.Parameters, lx, ly) && ExpressionsEqual(lx.Body, ly.Body, lx, ly);

                case MemberExpression mex:
                    var mey = (MemberExpression)y;
                    return Equals(mex.Member, mey.Member) && ExpressionsEqual(mex.Expression, mey.Expression, rootX, rootY);

                case BinaryExpression bx:
                    var by = (BinaryExpression)y;
                    return bx.Method == @by.Method
                        && ExpressionsEqual(bx.Left, @by.Left, rootX, rootY)
                        && ExpressionsEqual(bx.Right, @by.Right, rootX, rootY);

                case UnaryExpression ux:
                    var uy = (UnaryExpression)y;
                    return ux.Method == uy.Method && ExpressionsEqual(ux.Operand, uy.Operand, rootX, rootY);

                case ParameterExpression px:
                    return rootX.Parameters.IndexOf(px) == rootY.Parameters.IndexOf((ParameterExpression)y);

                case MethodCallExpression mcx:
                    var mcy = (MethodCallExpression)y;
                    return mcx.Method == mcy.Method
                        && ExpressionsEqual(mcx.Object, mcy.Object, rootX, rootY)
                        && CollectionsEqual(mcx.Arguments, mcy.Arguments, rootX, rootY);

                case MemberInitExpression mix:
                    var miy = (MemberInitExpression)y;
                    return ExpressionsEqual(mix.NewExpression, miy.NewExpression, rootX, rootY)
                        && MemberInitsEqual(mix.Bindings, miy.Bindings, rootX, rootY);

                case NewArrayExpression nax:
                    return CollectionsEqual(nax.Expressions, ((NewArrayExpression)y).Expressions, rootX, rootY);

                case NewExpression nx:
                    var ny = (NewExpression)y;
                    return Equals(nx.Constructor, ny.Constructor)
                        && CollectionsEqual(nx.Arguments, ny.Arguments, rootX, rootY)
                        && (nx.Members == null && ny.Members == null || nx.Members != null && ny.Members != null && CollectionsEqual(nx.Members, ny.Members));

                case ConditionalExpression cx:
                    var cy = (ConditionalExpression)y;
                    return ExpressionsEqual(cx.Test, cy.Test, rootX, rootY)
                        && ExpressionsEqual(cx.IfFalse, cy.IfFalse, rootX, rootY)
                        && ExpressionsEqual(cx.IfTrue, cy.IfTrue, rootX, rootY);


                default:
                    throw new NotImplementedException(x.ToString());
            }
        }

        private static bool IsAnonymousType(Type type)
            => type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any()
            && type.FullName.Contains("AnonymousType");

        private static bool MemberInitsEqual(ICollection<MemberBinding> bx, ICollection<MemberBinding> by, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (bx.Count != by.Count)
            {
                return false;
            }

            if (bx.Concat(by).Any(b => b.BindingType != MemberBindingType.Assignment))
            {
                throw new NotImplementedException("Only MemberBindingType.Assignment is supported");
            }

            return bx.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i })
                .Join(by.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i }),
                      o => o.Index, o => o.Index, (xe, ye) => new { XExpr = xe.Expr, XMember = xe.Member, YExpr = ye.Expr, YMember = ye.Member })
                       .All(o => Equals(o.XMember, o.YMember) && ExpressionsEqual(o.XExpr, o.YExpr, rootX, rootY));
        }

        private static bool ValuesEqual(object x, object y)
            => ReferenceEquals(x, y)
            || (x is ICollection xc && y is ICollection yc && CollectionsEqual(xc, yc))
            || (x is IEnumerable xe && y is IEnumerable ye && EnumerablesEqual(xe, ye))
            || Equals(x, y);

        private static ConstantValue TryCalculateConstant(Expression e)
        {
            switch (e)
            {
                case ConstantExpression constant:
                    return new ConstantValue(true, constant.Value);

                case MemberExpression me:
                    var support = me.Expression == null ? null : TryCalculateConstant(me.Expression).Value;

                    if (me.Member is FieldInfo fieldInfo)
                    {
                        return new ConstantValue(true, fieldInfo.GetValue(support));
                    }

                    if (me.Member is PropertyInfo propertyInfo)
                    {
                        return new ConstantValue(true, propertyInfo.GetValue(support));
                    }

                    break;

                case NewArrayExpression ae:
                    var result = ae.Expressions.Select(TryCalculateConstant);
                    if (result.All(i => i.IsDefined))
                    {
                        return new ConstantValue(true, result.Select(i => i.Value).ToArray());
                    }
                    break;

                case ConditionalExpression ce:
                    var evaluatedTest = TryCalculateConstant(ce.Test);
                    if (evaluatedTest.IsDefined)
                    {
                        return TryCalculateConstant(Equals(evaluatedTest.Value, true) ? ce.IfTrue : ce.IfFalse);
                    }
                    break;
            }

            return default;
        }

        private static bool CollectionsEqual(IEnumerable<Expression> x, IEnumerable<Expression> y, LambdaExpression rootX, LambdaExpression rootY)
            => x.Count() == y.Count()
            && x.Select((e, i) => new { Expr = e, Index = i })
                .Join(y.Select((e, i) => new { Expr = e, Index = i }),
                        o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
                .All(o => ExpressionsEqual(o.X, o.Y, rootX, rootY));

        private static bool EnumerablesEqual(IEnumerable x, IEnumerable y)
            => new HashSet<object>(x.Cast<object>()).SetEquals(new HashSet<object>(y.Cast<object>()));

        private static bool CollectionsEqual(ICollection x, ICollection y)
            => x.Count == y.Count
            && x.Cast<object>().Select((e, i) => new { Expr = e, Index = i })
                .Join(y.Cast<object>().Select((e, i) => new { Expr = e, Index = i }),
                        o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
                .All(o => Equals(o.X, o.Y));

        private struct ConstantValue
        {
            public ConstantValue(bool isDefined, object value)
                : this()
            {
                IsDefined = isDefined;
                Value = value;
            }

            public bool IsDefined { get; private set; }

            public object Value { get; private set; }
        }
    }
}