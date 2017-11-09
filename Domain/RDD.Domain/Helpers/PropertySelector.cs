using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using NExtends.Primitives;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;

namespace RDD.Domain.Helpers
{
    public class PropertySelector
    {
        protected PropertySelector()
        {
        }

        /// <summary>
        ///   WARNING ! This property flattens collection (i.e : if the actual property type was 'string[]', this would return 'string')
        /// </summary>
        public Type EntityType { get; protected set; }

        public PropertyInfo[] EntityProperties => EntityType.GetFlattenProperties(BindingFlags.NonPublic);

        public LambdaExpression Lambda { get; set; }
        public ISet<PropertySelector> Children { get; protected set; }

        public string Name
        {
            get
            {
                if (Lambda.Body is MemberExpression)
                {
                    return (Lambda.Body as MemberExpression).Member.Name;
                }

                if (Lambda.Body is MethodCallExpression)
                {
                    return (Lambda.Body as MethodCallExpression).Method.Name;
                }

                throw new NotImplementedException();
            }
        }

        public string Subject { get; set; }

        public int LeafNumber
        {
            get { return HasChild ? Children.Sum(c => c.LeafNumber) : 1; }
        }

        public int Count => Children.Count;

        public bool IsEmpty => Count == 0;

        public int CollectionCount => throw new NotImplementedException();

        public bool HasChild => Children.Any();

        public PropertySelector this[Expression<Func<object, object>> key]
        {
            get { return Children.FirstOrDefault(c => c.IsEqual(key)); }
        }

        /// <summary>
        ///   Pour les sous noeud, on instancie le type et on ajoutera les expressions via des .Add()
        ///   Ne pas modifier cette méthode, elle est utilisée  via Reflection, ou alors harmoniser la reflection en fonction
        /// </summary>
        public static PropertySelector<TEntity> New<TEntity>() => new PropertySelector<TEntity>();

        /// <summary>
        ///   Permet de récupérer un PropertySelector&lt;TEntity&gt; typé à partir d'un Type en param même si la signature ne le
        ///   laisse pas penser
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns>PropertySelector&lt;TEntity&gt; que vous pouvez caster</returns>
        public static PropertySelector NewFromType(Type entityType) => (PropertySelector) typeof(PropertySelector).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { });

        public bool Contains(LambdaExpression expression)
        {
            if (!ChildrenContains(expression))
            {
                var rootLambda = (LambdaExpression) new PropertySelectorRootLambdaExtractor().Visit(expression);

                PropertySelector matchingChild = GetChild(rootLambda);

                if (matchingChild != null)
                {
                    string propertyName = ((PropertyInfo) (matchingChild.Lambda.Body as MemberExpression).Member).Name;
                    var transferor = new PropertySelectorTransferor(EntityType, matchingChild.EntityType, propertyName);
                    var subExpression = (LambdaExpression) transferor.Visit(expression);

                    return matchingChild.Contains(subExpression);
                }

                return false;
            }

            return true;
        }

        public bool ContainsEmpty(LambdaExpression expression)
        {
            bool selfContains = IsEqual(expression);
            PropertySelector matchingChild = GetChild(expression);

            return matchingChild != null && matchingChild.IsEmpty;
        }

        public PropertySelector GetChild(LambdaExpression expression) => Children.FirstOrDefault(c => c.IsEqual(expression));

        protected bool ChildrenContains(LambdaExpression expression) => GetChild(expression) != null;

        protected bool AreEqual(LambdaExpression exp1, LambdaExpression exp2)
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

            //ICollection<TSub> on s'intéresse à TSub
            Type propertyType = property.PropertyType.GetEnumerableOrArrayElementType();

            //On regarde si le child n'existe pas déjà, auquel cas pas besoin de le recréer à chaque fois !
            PropertySelector matchingChild = Children.FirstOrDefault(c => c.IsEqual(lambda));

            if (matchingChild == null)
            {
                matchingChild = NewFromType(propertyType);
                matchingChild.Lambda = lambda;

                Children.Add(matchingChild);
            }

            if (tail.Any())
            {
                matchingChild.Parse(tail, depth + 1);
            }
        }

        public bool Add(LambdaExpression expression)
        {
            var extractor = new PropertySelectorRootLambdaExtractor();

            var rootLambda = (LambdaExpression) extractor.Visit(expression);
            PropertySelector matchingChild = GetChild(rootLambda);
            bool creationNeeded = matchingChild == null;

            Type propertyType = rootLambda.Body.Type;

            if (propertyType.IsGenericType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            if (creationNeeded)
            {
                matchingChild = NewFromType(propertyType);
                matchingChild.Lambda = rootLambda;

                Children.Add(matchingChild);
            }

            //L'expression concerne le selector courant (u => u.Id)
            if (AreEqual(rootLambda, expression))
            {
                return creationNeeded;
            }
            //On transfert l'expression sur le child
            var transferor = new PropertySelectorTransferor(EntityType, propertyType, ((PropertyInfo) ((MemberExpression) rootLambda.Body).Member).Name);

            return matchingChild.Add((LambdaExpression) transferor.Visit(expression));
        }

        public bool Remove(LambdaExpression expression)
        {
            var extractor = new PropertySelectorRootLambdaExtractor();

            var rootLambda = (LambdaExpression) extractor.Visit(expression);

            //L'expression concerne le selector courant (u => u.Id)
            if (IsEqual(expression))
            {
                //Rien à faire ici car ça veut dire que TOUS les enfants doivent être supprimés, et ils le seront
                //Quand l'appelant supprimera ce selector
                return true;
            }
            PropertySelector matchingChild = GetChild(rootLambda);

            if (matchingChild != null)
            {
                bool result = matchingChild.Remove(expression);

                Children.Remove(matchingChild);

                return result;
            }

            return false;
        }

        public override string ToString() => Lambda.ToString();

        /// <summary>
        ///   On conserve uniquement les Children qui descendent de l'interface
        ///   NB : utilisé notamment pour obtenir facilement les Includes EF à partir des Fields
        /// </summary>
        /// <returns></returns>
        public virtual PropertySelector CropToInterface(Type interfaceType)
        {
            if (!EntityType.IsSubclassOfInterface(interfaceType))
            {
                return null;
            }

            PropertySelector result = NewFromType(EntityType);
            result.Lambda = Lambda;

            if (HasChild)
            {
                result.Children = new HashSet<PropertySelector>(Children.Select(c => c.CropToInterface(interfaceType)).Where(c => c != null));
            }

            return result;
        }

        public List<string> ExtractPaths() => RecursiveExtractPaths(new List<string>(), this, new List<string>());

        private List<string> RecursiveExtractPaths(List<string> result, PropertySelector includes, IEnumerable<string> elementsOfpath)
        {
            foreach (PropertySelector child in includes.Children)
            {
                //u => u.Department
                //On prend le body, donc u.Department
                //On vire le u.
                //Il ne reste que "Department"
                //NB : le vrai Include() typé ne fonctionne pas car il faut lui préciser le type de la propriété at compile time !
                //Et de toute façon, en lisant la doc de l'include typé, on voit qu'il appelle aussi l'include non typé !
                IEnumerable<string> elements = child.Lambda.Body.ToString().Split('.');
                elements = elementsOfpath.Union(new[] {elements.ElementAt(1)});

                //Si le noeud a des enfants, ce n'est qu'un intermédiaire, on va donc uniquement inclure ses enfants, il sera inclus nativement lui-même par EF
                if (child.HasChild)
                {
                    result = RecursiveExtractPaths(result, child, elements);
                }
                else
                {
                    result.Add(string.Join(".", elements));
                }
            }

            return result;
        }
    }

    /// <summary>
    ///   Correspond à une Expression de sélection d'une propriété
    ///   Avec une info qui permet de savoir si c'est un type "final" (string, DateTime, ..) ou pas
    /// </summary>
    public class PropertySelector<TEntity> : PropertySelector
    {
        /// <summary>
        ///   Utilisé pour la racine de l'arbre, il n'a pas besoin d'expression
        /// </summary>
        /// <param name="entityType"></param>
        public PropertySelector()
        {
            EntityType = typeof(TEntity);
            Children = new HashSet<PropertySelector>();
        }

        public PropertySelector(params Expression<Func<TEntity, object>>[] expressions)
            : this()
        {
            Add(expressions);
        }

        public bool Contains(Expression<Func<TEntity, object>> expression) => Contains((LambdaExpression) expression);

        public PropertySelector<TSub> GetChild<TSub>(Expression<Func<TEntity, TSub>> expression) => (PropertySelector<TSub>) GetChild((LambdaExpression) expression);

        public PropertySelector<TSub> GetChild<TSub>(Expression<Func<TEntity, IEnumerable<TSub>>> expression) => (PropertySelector<TSub>) GetChild((LambdaExpression) expression);

        public bool ContainsAny(params Expression<Func<TEntity, object>>[] expressions)
        {
            return expressions.Any(Contains);
        }

        public bool Add(params Expression<Func<TEntity, object>>[] expressions)
        {
            return expressions.Select(expression => { return Add((LambdaExpression) expression); }).Aggregate((b1, b2) => b1 && b2);
        }

        public bool Add(Expression<Func<TEntity, object>> exp) => Add((LambdaExpression) exp);

        public bool Add<TSub>(PropertySelector<TSub> child, LambdaExpression selector)
        {
            //Le child a été construit comme un selecteur racine, donc avec un Lambda null
            //Ici il va devrenir enfant d'un selecteur, donc il faut patcher son Lambda
            child.Lambda = selector;

            PropertySelector matchingChild = Children.FirstOrDefault(c => c.Lambda.Body.Type == typeof(TSub));

            if (matchingChild == null)
            {
                Children.Add(child);

                return true;
            }
            Children.Remove(matchingChild);
            Children.Add(child);

            return false;
        }

        public bool Remove(Expression<Func<TEntity, object>> exp) => Remove((LambdaExpression) exp);

        /// <summary>
        ///   Permet de transférer un selecteur vers un enfant
        ///   NB : utiliser p. comme paramètre du selecteur
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public PropertySelector<TSub> TransfertTo<TSub>(LambdaExpression selector)
        {
            PropertySelector matchingChild = Children.FirstOrDefault(c => c.IsEqual(selector));

            if (matchingChild == null)
            {
                throw new Exception(string.Format("Child {0} not found on type {1}. Notice that you must use p. parameter in your selector !", selector.ToString(), typeof(TEntity).Name));
            }

            return (PropertySelector<TSub>) matchingChild;
        }

        public PropertySelector<TBase> Cast<TBase>()
        {
            var result = new PropertySelector<TBase>();

            //var param = Expression.Parameter(result.EntityType, "p");

            result.Lambda = Lambda; //Expression.Lambda(Lambda.Body)

            result.Children = Children;

            return result;
        }
    }
}