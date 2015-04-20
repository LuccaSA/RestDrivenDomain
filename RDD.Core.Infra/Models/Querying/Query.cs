using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	//http://stackoverflow.com/questions/2650080/how-to-get-c-sharp-enum-description-from-value
	public enum Reserved
	{
		[Description("Champ utilisé par jQuery")]
		_,
		[Description("Champ utilisé pour le cache")]
		randomnumber,
		[Description("Champ utilisé pour l'authentification")]
		authToken,
		[Description("Champ utilisé pour l'appel de fonction callback")]
		callback,
		[Description("Champ utilisé pour une demande explicite de fields")]
		fields,
		[Description("Champ utilisé pour faire des groupby")]
		groupby,
		[Description("Champ utilisé pour les post?httpmethod=PUT")]
		httpmethod,
		[Description("Champ utilisé pour l'authentification")]
		longtoken,
		[Description("Champ utilisé en cas de besoin d'aide exemple : ?metadata")]
		metadata,
		[Description("Permet de bloquer la notification par mail lors d'un appel à l'API : &notify=false")]
		notify,
		[Description("Champ utilisé pour filtrer en fonction des opérations accessibles")]
		operations,
		[Description("Champ utilisé en cas de besoin d'odonner les réultats")]
		orderby,
		[Description("Champ utilisé en cas de besoin de paging")]
		paging,
		[Description("Permet de filtrer les ressources selon les droits de vision de ce principal et pas de curPrincipal => en réalité on fait un Inter() évidemment")]
		principal,
		[Description("Champ utilisé pour montrer le template -> ne renvoie rien pour le moment")]
		template,
		[Description("Champ utilisé pour montrer type")]
		type
	}

	public class Query<T>
	{
		public List<OrderBy> OrderBys { get; set; }
		public Expression<Func<T, bool>> ExpressionFilters { get; set; }
		public ICollection<Filter> Filters { get; set; }
		public Field Fields { get; set; }
		public Options Options { get; set; }
		public ICollection<string> Includes { get; set; }

		public Query()
		{
			OrderBys = new List<OrderBy>();
			Options = new Options();
			Includes = new HashSet<string>();
			Filters = new List<Filter>();
		}

		public Query(Field fields, bool isCollectionCall = true)
			: this()
		{
			Fields = fields;
			SetIncludeFromFields(isCollectionCall);
		}

		public Query<T> Parse(IWebContext webContext, bool isCollectionCall = true)
		{
			//On transforme la queryString en PostedData pour que ce soit plus simple à manipuler ensuite
			var parameters = PostedData.ParseDictionary(webContext.GetQueryNameValuePairs().ToDictionary(K => K.Key.ToLower(), K => K.Value));

			//On s'occupe des orderbys
			if (parameters.ContainsKey(Reserved.orderby))
			{
				OrderBys = OrderBy.Parse(parameters[Reserved.orderby].value);
			}

			//Puis des wheres
			Filters = Filter.Parse<T>(parameters);

			//Le filtre sur certaines opérations
			if (parameters.ContainsKey(Reserved.operations + ".id")
				|| parameters.ContainsKey(Reserved.operations + ".name"))
			{
				Options.FilterOperations = parameters;
			}

			//Si on doit renvoyer une réponse partielle
			if (parameters.ContainsKey(Reserved.fields))
			{
				Fields = Field.Parse(parameters[Reserved.fields].value);

				//Si les fields demandent des propriétés sur la collection
				if (Fields.ContainsKey("collection"))
				{
					//Et uniquement sur la collection ?
					if (Fields.Count == 1)
					{
						//Alors pas besoin d'énumérer les entités
						Options.NeedEnumeration = false;
					}

					if (Fields["collection"].ContainsKey("count"))
					{
						Options.NeedCount = true;
					}
				}
			}
			else if (isCollectionCall)
			{
				Fields = Field.Parse("id, name, url");
			}
			else
			{
				Fields = Field.Parse(String.Join(", ", typeof(T).GetProperties().Select(p => p.Name).ToArray()));
			}

			//Paging
			if (parameters.ContainsKey(Reserved.paging))
			{
				Options.Page = Page.Parse(parameters[Reserved.paging].value);
				Options.withPaging = true;
			}

			SetIncludeFromFields(isCollectionCall);

			return this;
		}

		void SetIncludeFromFields(bool isCollectionCall)
		{
			RecursiveInclude(Fields, typeof(T), new string[] { });
		}

		void RecursiveInclude(Field field, Type entityType, string[] propertyTree)
		{
			var propertyPath = "";
			if (propertyTree.Any())
			{
				propertyPath = String.Join(".", propertyTree) + ".";
			}

			foreach (var key in field.Keys)
			{
				var property = GetPropertyFromKey(entityType, key);

				if (property != null)
				{
					if (IsIncludeCandidate(property))
					{
						var propertyType = property.PropertyType.GetListOrArrayElementType(); //ICollection<LegalEntity> => LegalEntity

						//On regarde si y'a pas des subs qui sont eux mêmes des EntityBase
						if (field[key].subs.Keys.Any(sub =>
						{
							var subProperty = GetPropertyFromKey(propertyType, sub);
							if (subProperty != null)
							{
								return IsIncludeCandidate(subProperty);
							}
							return false;
						}))
						{
							RecursiveInclude(field[key], propertyType, propertyTree.Concat(new string[] { property.Name }).ToArray());
						}
						else //Si on est au bout d'une branche, on joue le Include
						{
							IncludeProperty(propertyPath, property);
						}
					}
				}
			}
		}

		protected virtual void IncludeProperty(string propertyPath, PropertyInfo property)
		{
			Includes.Add(propertyPath + property.Name);
		}

		private static PropertyInfo GetPropertyFromKey(Type type, string key)
		{
			return type.GetProperties().Where(p => p.Name.ToLower() == key).FirstOrDefault();
		}

		protected virtual bool IsIncludeCandidate(PropertyInfo property)
		{
			return property.PropertyType.IsSubclassOfInterface(typeof(IIncludable))
						|| (property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments()[0].IsSubclassOfInterface(typeof(IIncludable)));
		}
	}
}