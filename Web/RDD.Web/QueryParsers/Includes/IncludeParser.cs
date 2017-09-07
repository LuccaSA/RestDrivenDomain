using RDD.Domain.Models.Querying;
using RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees;
using System.Collections.Generic;

namespace RDD.Web.QueryParsers.Includes
{
	class IncludeParser<T>
	{
		public SelectorsTreeRoot<T> ParseIncludes(Dictionary<string, string> parameters, bool isCollectionCall)
		{
			//Si on doit renvoyer une réponse partielle
			if (parameters.ContainsKey(Reserved.fields.ToString()))
			{
				return new SelectorsTreeParser().Parse<T>(new TreeParser().Parse(parameters[Reserved.fields.ToString()]));
			}
			else if (!isCollectionCall)
			{
				return new SelectorsTreeParser().ParseAllPropertie<T>();
			}
			return new SelectorsTreeRoot<T>();
		}
	}
}
