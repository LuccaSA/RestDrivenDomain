using RDD.Domain;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.QueryParsers
{
	class OptionsParser
	{
		public Options Parse<T>(Dictionary<string, string> parameters, Field<T> fields)
		{
			var result = new Options();

			//Si les fields demandent des propriétés sur la collection
			if (fields.CollectionContains<T>(c => c.Count))
			{
				result.NeedCount = true;

				//Et uniquement sur le count de la collection ?
				if (fields.Count == 0 && fields.CollectionCount == 1)
				{
					//Alors pas besoin d'énumérer les entités
					result.NeedEnumeration = false;
				}
			}

			//Le filtre sur certaines opérations
			if (parameters.ContainsKey(Reserved.operations + ".id")
				|| parameters.ContainsKey(Reserved.operations + ".name"))
			{
				result.FilterOperations = parameters;
			}

			//Impersonation d'un autre principal
			if (parameters.ContainsKey(Reserved.principal.ToString() + ".id"))
			{
				result.impersonatedPrincipal = int.Parse(parameters[Reserved.principal.ToString() + ".id"]);
			}

			result.attachActions = fields.Contains(e => ((IEntityBase)e).AuthorizedActions);
			result.attachOperations = result.attachActions || fields.Contains(e => ((IEntityBase)e).AuthorizedOperations);

			//Paging
			if (parameters.ContainsKey(Reserved.paging.ToString()))
			{
				result.Page = Page.Parse(parameters[Reserved.paging.ToString()]);
			}

			//No Warnings
			if (parameters.ContainsKey(Reserved.nowarning.ToString()))
			{
				if (parameters[Reserved.nowarning.ToString()] == "1")
				{
					result.withWarnings = false;
				}
			}

			//Accept
			if (parameters.ContainsKey(Reserved.accept.ToString()))
			{
				result.Accept = parameters[Reserved.accept.ToString()];
			}

			return result;
		}
	}
}