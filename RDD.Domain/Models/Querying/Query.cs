using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models.Querying
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
		[Description("Champ utilisé pour récupérer les données dans un autre format: &accept=application/xls")]
		accept,
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
		[Description("Champ utilisé pour forcer une action")]
		nowarning
	}

	public class Query<TEntity>
		where TEntity : class, IEntityBase
	{
		public HttpVerb Verb { get; set; }
		public List<OrderBy> OrderBys { get; set; }
		public Expression<Func<TEntity, bool>> ExpressionFilters { get; set; }
		public ICollection<Where> Filters { get; set; }
		public Func<TEntity, object> ExpressionFieldsSelector { get; set; }
		public Field<TEntity> Fields { get; set; }
		public virtual Options Options { get; set; }
		public PropertySelector<TEntity> Includes { get; set; }

		protected HashSet<string> IgnoredFilters { get; set; }

		public Query()
		{
			Verb = HttpVerb.GET;
			OrderBys = new List<OrderBy>();
			Options = new Options();
			Filters = new List<Where>();
			Includes = new PropertySelector<TEntity>();
			Fields = new Field<TEntity>();
			IgnoredFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		public Query(Expression<Func<TEntity, object>> field, bool isCollectionCall = true)
			: this()
		{
			Fields.Add(field);
		}

		public Query(string fields, bool isCollectionCall = true)
			: this()
		{
			var data = PostedData.ParseUrlEncoded(String.Format("fields={0}", fields));

			Parse(data, isCollectionCall);
		}

		public void IgnoreFilters(params string[] filters)
		{
			foreach (var filter in filters)
			{
				IgnoredFilters.Add(filter);
			}
		}

		public Query<TEntity> Parse(IWebContext webContext, bool isCollectionCall = true)
		{
			//On transforme la queryString en PostedData pour que ce soit plus simple à manipuler ensuite
			var parameters = PostedData.ParseDictionary(webContext.GetQueryNameValuePairs().Where(v => !IgnoredFilters.Contains(v.Key)).ToDictionary(K => K.Key.ToLower(), K => K.Value));

			return Parse(parameters, isCollectionCall);
		}

		protected virtual Query<TEntity> Parse(PostedData parameters, bool isCollectionCall = true)
		{
			//On s'occupe des orderbys
			if (parameters.ContainsKey(Reserved.orderby))
			{
				OrderBys = OrderBy.Parse(parameters[Reserved.orderby].value);
			}

			//Puis des wheres
			Filters = Where.Parse<TEntity>(parameters);

			//Le filtre sur certaines opérations
			if (parameters.ContainsKey(Reserved.operations + ".id")
				|| parameters.ContainsKey(Reserved.operations + ".name"))
			{
				Options.FilterOperations = parameters;
			}

			//Impersonation d'un autre principal
			if (parameters.ContainsKey(Reserved.principal.ToString() + ".id"))
			{
				Options.impersonatedPrincipal = int.Parse(parameters[Reserved.principal.ToString() + ".id"].value);
			}

			//Si on doit renvoyer une réponse partielle
			if (parameters.ContainsKey(Reserved.fields))
			{
				Fields.Parse(parameters[Reserved.fields].value);

				//Si les fields demandent des propriétés sur la collection
				if (Fields.CollectionContains<TEntity>(c => c.Count))
				{
					Options.NeedCount = true;

					//Et uniquement sur le count de la collection ?
					if (Fields.Count == 0 && Fields.CollectionCount == 1)
					{
						//Alors pas besoin d'énumérer les entités
						Options.NeedEnumeration = false;
					}
				}
			}
			else if (!isCollectionCall)
			{
				Fields.ParseAllProperties();
			}
			//            query.ExpressionFieldsSelector = new ExpressionFieldsService().GetExpression<TEntity>(query.Fields);

			Options.attachActions = Fields.Contains(e => ((IEntityBase)e).Actions);
			Options.attachOperations = Options.attachActions || Fields.Contains(e => ((IEntityBase)e).Operations);

			//Paging
			if (parameters.ContainsKey(Reserved.paging))
			{
				Options.Page = Page.Parse(parameters[Reserved.paging].value);
				Options.withPagingInfo = true;
			}

			//No Warnings
			if (parameters.ContainsKey(Reserved.nowarning))
			{
				if (parameters[Reserved.nowarning].value == "1")
				{
					Options.withWarnings = false;
				}
			}

			//Accept
			if (parameters.ContainsKey(Reserved.accept))
			{
				Options.Accept = parameters[Reserved.accept].value;
			}

			//On détecte les .Include() à rajouter en fonction des fieds demandés
			Includes = GetIncludesFromFields(Fields);

			return this;
		}

		protected virtual PropertySelector<TEntity> GetIncludesFromFields(Field<TEntity> fields)
		{
			return (PropertySelector<TEntity>)fields.EntitySelector.CropToInterface(typeof(IIncludable));
		}
	}
}
