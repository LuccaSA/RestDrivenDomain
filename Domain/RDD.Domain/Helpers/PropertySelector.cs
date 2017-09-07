using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NExtends.Primitives;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;

namespace RDD.Domain.Helpers
{
	public class PropertySelector
	{
		/// <summary>
		/// WARNING ! This property flattens collection (i.e : if the actual property type was 'string[]', this would return 'string')
		/// </summary>
		public Type EntityType { get; protected set; }
		public PropertyInfo[] EntityProperties { get { return EntityType.GetFlattenProperties(BindingFlags.NonPublic); } }

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
			get
			{
				return (HasChild ? Children.Sum(c => c.LeafNumber) : 1);
			}
		}

		protected PropertySelector() { }

		/// <summary>
		/// Pour les sous noeud, on instancie le type et on ajoutera les expressions via des .Add()
		/// Ne pas modifier cette méthode, elle est utilisée  via Reflection, ou alors harmoniser la reflection en fonction
		/// </summary>
		/// <param name="entityType"></param>
		public static PropertySelector<TEntity> New<TEntity>()
		{
			return new PropertySelector<TEntity>();
		}
		/// <summary>
		/// Permet de récupérer un PropertySelector&lt;TEntity&gt; typé à partir d'un Type en param même si la signature ne le laisse pas penser
		/// </summary>
		/// <param name="entityType"></param>
		/// <returns>PropertySelector&lt;TEntity&gt; que vous pouvez caster</returns>
		public static PropertySelector NewFromType(Type entityType)
		{
			return (PropertySelector)typeof(PropertySelector).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { });
		}

		public int Count { get { return Children.Count; } }

		public bool IsEmpty { get { return Count == 0; } }

		public int CollectionCount { get { throw new NotImplementedException(); } }

		public bool Contains(LambdaExpression expression)
		{
			if (!ChildrenContains(expression))
			{
				var rootLambda = (LambdaExpression)new PropertySelectorRootLambdaExtractor().Visit(expression);

				var matchingChild = GetChild(rootLambda);

				if (matchingChild != null)
				{
					var propertyName = ((PropertyInfo)(matchingChild.Lambda.Body as MemberExpression).Member).Name;
					var transferor = new PropertySelectorTransferor(EntityType, matchingChild.EntityType, propertyName);
					var subExpression = (LambdaExpression)transferor.Visit(expression);

					return matchingChild.Contains(subExpression);
				}

				return false;
			}

			return true;
		}

		public bool ContainsEmpty(LambdaExpression expression)
		{
			var selfContains = IsEqual(expression);
			var matchingChild = GetChild(expression);

			return matchingChild != null && matchingChild.IsEmpty;
		}

		public PropertySelector GetChild(LambdaExpression expression)
		{
			return Children.Where(c => c.IsEqual(expression)).FirstOrDefault();
		}

		protected bool ChildrenContains(LambdaExpression expression)
		{
			return GetChild(expression) != null;
		}

		protected bool AreEqual(LambdaExpression exp1, LambdaExpression exp2)
		{
			if (exp1 == null && exp2 == null) { return true; }
			else if (exp1 == null || exp2 == null) { return false; }
			else
			{
				Func<string, string> regex = (expressionString) =>
				{
					var result = Regex.Replace(expressionString, "Convert\\(([\\w\\.?]*),?.*\\)", "$1"); //Convert(p.Id) => p.Id
					var elements = result.Split('.').ToList();
					elements.RemoveAt(0); //On vire le paramètre en préfixe, p.xx.Select(..) => xx.Select(..)
					result = String.Join(".", elements);

					return result;
				};

				return regex(exp1.Body.ToString()) == regex(exp2.Body.ToString());
			}
		}

		public bool IsEqual(LambdaExpression exp)
		{
			return AreEqual(exp, Lambda);
		}

		protected virtual PropertyInfo GetProperty(string propertyName)
		{
			var property = EntityProperties.FirstOrDefault(p => p.Name.ToLower() == propertyName.ToLower());

			if (property == null)
			{
				throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type {1}", propertyName, EntityType.Name));
			}

			return property;
		}

		public PropertyInfo GetCurrentProperty()
		{
			if (Lambda == null)
				return null;

			var member = Lambda.Body as MemberExpression;
			if (member == null)
				return null;

			return member.Member as PropertyInfo;
		}

		public bool HasChild { get { return Children.Any(); } }

		public PropertySelector this[Expression<Func<object, object>> key]
		{
			get { return Children.Where(c => c.IsEqual(key)).FirstOrDefault(); }
		}

		/// <summary>
		/// Ex : department.users.name sur le type User
		/// </summary>
		/// <param name="field"></param>
		public virtual void Parse(string field)
		{
			var elements = field.Split('.').ToList();

			Parse(elements, 1);
		}

		public void Parse(List<string> elements, int depth)
		{
			var first = elements[0];
			elements.RemoveAt(0);

			Parse(first, elements, depth);
		}

		public virtual void Parse(string element, List<string> tail, int depth)
		{
			var property = GetProperty(element);

			var param = Expression.Parameter(EntityType, "p".Repeat(depth));

			var member = Expression.Property(param, property);

			var lambda = Expression.Lambda(member, param);

			//ICollection<TSub> on s'intéresse à TSub
			var propertyType = property.PropertyType.GetEnumerableOrArrayElementType();

			//On regarde si le child n'existe pas déjà, auquel cas pas besoin de le recréer à chaque fois !
			var matchingChild = Children.Where(c => c.IsEqual(lambda)).FirstOrDefault();

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

			var rootLambda = (LambdaExpression)extractor.Visit(expression);
			var matchingChild = GetChild(rootLambda);
			var creationNeeded = (matchingChild == null);

			var propertyType = rootLambda.Body.Type;

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
			else //L'expression concerne un enfant du selector (u => u.Department.Name)
			{
				//On transfert l'expression sur le child
				var transferor = new PropertySelectorTransferor(EntityType, propertyType, ((PropertyInfo)((MemberExpression)rootLambda.Body).Member).Name);

				return matchingChild.Add((LambdaExpression)transferor.Visit(expression));
			}
		}

		public bool Remove(LambdaExpression expression)
		{
			var extractor = new PropertySelectorRootLambdaExtractor();

			var rootLambda = (LambdaExpression)extractor.Visit(expression);

			//L'expression concerne le selector courant (u => u.Id)
			if (IsEqual(expression))
			{
				//Rien à faire ici car ça veut dire que TOUS les enfants doivent être supprimés, et ils le seront
				//Quand l'appelant supprimera ce selector
				return true;
			}
			else //L'expression concerne un enfant du selector (u => u.Department.Name)
			{
				var matchingChild = GetChild(rootLambda);

				if (matchingChild != null)
				{
					var result = matchingChild.Remove(expression);

					Children.Remove(matchingChild);

					return result;
				}
			}

			return false;
		}

		public override string ToString()
		{
			return Lambda.ToString();
		}

		/// <summary>
		/// On conserve uniquement les Children qui descendent de l'interface
		/// NB : utilisé notamment pour obtenir facilement les Includes EF à partir des Fields
		/// </summary>
		/// <returns></returns>
		public virtual PropertySelector CropToInterface(Type interfaceType)
		{
			if (!EntityType.IsSubclassOfInterface(interfaceType))
			{
				return null;
			}

			var result = PropertySelector.NewFromType(EntityType);
			result.Lambda = Lambda;

			if (HasChild)
			{
				result.Children = new HashSet<PropertySelector>(Children.Select(c => c.CropToInterface(interfaceType)).Where(c => c != null));
			}

			return result;
		}

		public List<string> ExtractPaths()
		{
			return RecursiveExtractPaths(new List<string>(), this, new List<string>());
		}

		private List<string> RecursiveExtractPaths(List<string> result, PropertySelector includes, IEnumerable<string> elementsOfpath)
		{
			foreach (var child in includes.Children)
			{
				//u => u.Department
				//On prend le body, donc u.Department
				//On vire le u.
				//Il ne reste que "Department"
				//NB : le vrai Include() typé ne fonctionne pas car il faut lui préciser le type de la propriété at compile time !
				//Et de toute façon, en lisant la doc de l'include typé, on voit qu'il appelle aussi l'include non typé !
				IEnumerable<string> elements = child.Lambda.Body.ToString().Split('.');
				elements = elementsOfpath.Union(new string[] { elements.ElementAt(1) });

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
	/// Correspond à une Expression de sélection d'une propriété
	/// Avec une info qui permet de savoir si c'est un type "final" (string, DateTime, ..) ou pas
	/// </summary>
	public class PropertySelector<TEntity> : PropertySelector
	{
		/// <summary>
		/// Utilisé pour la racine de l'arbre, il n'a pas besoin d'expression
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
		public bool Contains(Expression<Func<TEntity, object>> expression)
		{
			return Contains((LambdaExpression)expression);
		}
		public PropertySelector<TSub> GetChild<TSub>(Expression<Func<TEntity, TSub>> expression)
		{
			return (PropertySelector<TSub>)GetChild((LambdaExpression)expression);
		}
		public PropertySelector<TSub> GetChild<TSub>(Expression<Func<TEntity, IEnumerable<TSub>>> expression)
		{
			return (PropertySelector<TSub>)GetChild((LambdaExpression)expression);
		}
		public bool ContainsAny(params Expression<Func<TEntity, object>>[] expressions)
		{
			return expressions.Any(e => Contains(e));
		}

		public bool Add(params Expression<Func<TEntity, object>>[] expressions)
		{
			return expressions.Select(expression =>
			{
				return Add((LambdaExpression)expression);
			}).Aggregate((b1, b2) => b1 && b2);
		}

		public bool Add(Expression<Func<TEntity, object>> exp)
		{
			return Add((LambdaExpression)exp);
		}
		public bool Add<TSub>(PropertySelector<TSub> child, LambdaExpression selector)
		{
			//Le child a été construit comme un selecteur racine, donc avec un Lambda null
			//Ici il va devrenir enfant d'un selecteur, donc il faut patcher son Lambda
			child.Lambda = selector;

			var matchingChild = Children.Where(c => c.Lambda.Body.Type == typeof(TSub)).FirstOrDefault();

			if (matchingChild == null)
			{
				Children.Add(child);

				return true;
			}
			else //Remplacement du child existant
			{
				Children.Remove(matchingChild);
				Children.Add(child);

				return false;
			}
		}
		public bool Remove(Expression<Func<TEntity, object>> exp)
		{
			return Remove((LambdaExpression)exp);
		}

		/// <summary>
		/// Permet de transférer un selecteur vers un enfant
		/// NB : utiliser p. comme paramètre du selecteur
		/// </summary>
		/// <typeparam name="TSub"></typeparam>
		/// <param name="selector"></param>
		/// <returns></returns>
		public PropertySelector<TSub> TransfertTo<TSub>(LambdaExpression selector)
		{
			var matchingChild = Children.Where(c => c.IsEqual(selector)).FirstOrDefault();

			if (matchingChild == null)
			{
				throw new Exception(String.Format("Child {0} not found on type {1}. Notice that you must use p. parameter in your selector !", selector.ToString(), typeof(TEntity).Name));
			}

			return (PropertySelector<TSub>)matchingChild;
		}

		public PropertySelector<TBase> Cast<TBase>()
		{
			var result = new PropertySelector<TBase>();

			//var param = Expression.Parameter(result.EntityType, "p");

			result.Lambda = Lambda;//Expression.Lambda(Lambda.Body)

			result.Children = Children;

			return result;
		}
	}
}
