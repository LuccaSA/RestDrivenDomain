using NExtends.Primitives.Strings;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RDD.Domain.Helpers
{
    public class PropertySelector
    {
        public PropertySelector(Type entityType)
        {
            EntityType = entityType;
        }
        public PropertySelector(Type entityType, LambdaExpression expression)
            : this(entityType)
        {
            Add(expression);
        }

        /// <summary>
        ///   WARNING ! This property flattens collection (i.e : if the actual property type was 'string[]', this would return 'string')
        /// </summary>
        public Type EntityType { get; protected set; }

        public PropertyInfo[] EntityProperties => EntityType.GetFlattenProperties(BindingFlags.NonPublic);

        public LambdaExpression Lambda { get; set; }
        public PropertySelector Child { get; protected set; }

        public string Name
        {
            get
            {
                switch (Lambda.Body)
                {
                    case MemberExpression me:
                        return me.Member.Name;
                    case MethodCallExpression ce:
                        return ce.Method.Name;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string Path
        {
            get
            {
                //Root
                if (Lambda == null)
                {
                    return Child.Path;
                }

                //Intermediate
                if (HasChild)
                {
                    return $"{Name}.{Child.Path}";
                }

                //Leaf
                return Name;
            }
        }

        public string Subject { get; set; }

        public int CollectionCount => throw new NotImplementedException();

        public bool HasChild => Child != null;

        /// <summary>
        ///   Pour les sous noeud, on instancie le type et on ajoutera les expressions via des .Add()
        ///   Ne pas modifier cette méthode, elle est utilisée  via Reflection, ou alors harmoniser la reflection en fonction
        /// </summary>
        public static PropertySelector<TEntity> New<TEntity>(LambdaExpression expression) => new PropertySelector<TEntity>(expression);

        /// <summary>
        ///   Permet de récupérer un PropertySelector&lt;TEntity&gt; typé à partir d'un Type en param même si la signature ne le
        ///   laisse pas penser
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns>PropertySelector&lt;TEntity&gt; que vous pouvez caster</returns>
        public static PropertySelector NewFromType(Type entityType, LambdaExpression expression)
        {
            if (entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var dictionaryGenericArguments = entityType.GetGenericArguments();
                var propertySelector = typeof(DictionaryPropertySelector<,>).MakeGenericType(dictionaryGenericArguments[0], dictionaryGenericArguments[1]);
                return (PropertySelector)Activator.CreateInstance(propertySelector, expression);
            }

            return (PropertySelector)typeof(PropertySelector).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { expression });
        }

        public bool Contains(LambdaExpression expression)
        {
            //Matching leaf
            if (IsEqual(expression))
            {
                return true;
            }

            //Non matching leaf
            if (!HasChild)
            {
                return false;
            }

            //Potential intermediate
            var rootLambda = (LambdaExpression)new PropertySelectorRootLambdaExtractor().Visit(expression);
            if (IsEqual(rootLambda))
            {
                string propertyName = ((PropertyInfo)(Lambda.Body as MemberExpression).Member).Name;
                var transferor = new PropertySelectorTransferor(EntityType, Child.EntityType, propertyName);
                var subExpression = (LambdaExpression)transferor.Visit(expression);

                //Recursive call upon child node
                return Child.Contains(subExpression);
            }

            //Non matching intermediate
            return false;
        }

        public static bool AreEqual(LambdaExpression exp1, LambdaExpression exp2)
        {
            if (exp1 == null && exp2 == null)
            {
                return true;
            }
            if (exp1 == null || exp2 == null)
            {
                return false;
            }
            Func<string, string> regex = expressionString =>
            {
                string result = Regex.Replace(expressionString, "Convert\\(([\\w\\.?]*),?.*\\)", "$1"); //Convert(p.Id) => p.Id
                List<string> elements = result.Split('.').ToList();
                elements.RemoveAt(0); //On vire le paramètre en préfixe, p.xx.Select(..) => xx.Select(..)
                result = string.Join(".", elements);

                return result;
            };

            return regex(exp1.Body.ToString()) == regex(exp2.Body.ToString());
        }

        public bool IsEqual(LambdaExpression exp) => AreEqual(exp, Lambda);

        protected virtual PropertyInfo GetProperty(string propertyName)
        {
            PropertyInfo property = EntityProperties.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase));

            if (property == null)
            {
                throw new BadRequestException(string.Format("Unknown property {0} on type {1}", propertyName, EntityType.Name));
            }

            return property;
        }

        public PropertyInfo GetCurrentProperty()
        {
            if (Lambda == null)
            {
                return null;
            }

            var member = Lambda.Body as MemberExpression;
            if (member == null)
            {
                return null;
            }

            return member.Member as PropertyInfo;
        }

        /// <summary>
        ///   Ex : department.users.name sur le type User
        /// </summary>
        /// <param name="field"></param>
        public virtual void Parse(string field)
        {
            List<string> elements = field.Split('.').ToList();

            Parse(elements, 1);
        }

        public void Parse(List<string> elements, int depth)
        {
            string first = elements[0];
            elements.RemoveAt(0);

            Parse(first, elements, depth);
        }

        public virtual void Parse(string element, List<string> tail, int depth)
        {
            PropertyInfo property = GetProperty(element);

            ParameterExpression param = Expression.Parameter(EntityType, "p".Repeat(depth));

            MemberExpression member = Expression.Property(param, property);

            LambdaExpression lambda = Expression.Lambda(member, param);

            Lambda = lambda;

            if (tail.Any())
            {
                //ICollection<TSub> on s'intéresse à TSub
                Type propertyType = property.PropertyType.GetEnumerableOrArrayElementType();

                Child = NewFromType(propertyType, null);
                Child.Parse(tail, depth + 1);
            }
        }

        protected bool Add(LambdaExpression expression)
        {
            var extractor = new PropertySelectorRootLambdaExtractor();
            Lambda = (LambdaExpression)extractor.Visit(expression);

            //L'expression concerne le selector courant (u => u.Id), on sort
            if (AreEqual(Lambda, expression))
            {
                return true;
            }

            //L'expression doit être transférer sur l'enfant
            var property = (PropertyInfo)((MemberExpression)Lambda.Body).Member;
            var propertyType = property.PropertyType;
            if (propertyType.IsGenericType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }
            var transferor = new PropertySelectorTransferor(EntityType, propertyType, property.Name);

            Child = NewFromType(propertyType, (LambdaExpression)transferor.Visit(expression));

            return true;
        }

        public override string ToString() => Lambda.ToString();
    }

    /// <summary>
    ///   Correspond à une Expression de sélection d'une propriété
    ///   Avec une info qui permet de savoir si c'est un type "final" (string, DateTime, ..) ou pas
    /// </summary>
    public class PropertySelector<TEntity> : PropertySelector
    {
        public PropertySelector() : base(typeof(TEntity)) { }
        public PropertySelector(LambdaExpression expression)
            : base(typeof(TEntity), expression) { }

        public PropertySelector(Expression<Func<TEntity, object>> expression)
            : this((LambdaExpression)expression) { }

        public bool Contains(Expression<Func<TEntity, object>> expression) => Contains((LambdaExpression)expression);

        public bool ContainsAny(params Expression<Func<TEntity, object>>[] expressions)
        {
            return expressions.Any(Contains);
        }
    }
}