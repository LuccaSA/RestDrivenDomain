using RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees;
using System.Collections.Generic;
using System.ComponentModel;

namespace RDD.Domain.Models.Querying
{
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
	{
		public List<OrderBy> OrderBys { get; set; }
		public ICollection<Where> Filters { get; set; }
		public Field<TEntity> Fields { get; set; }
		public Headers Headers { get; set; }
		public SelectorsTreeRoot<TEntity> Includes { get; set; }

		public virtual Options Options { get; set; }

		public Query()
		{
			OrderBys = new List<OrderBy>();
			Options = new Options();
			Headers = new Headers();
			Filters = new List<Where>();
			Fields = new Field<TEntity>();
			Includes = new SelectorsTreeRoot<TEntity>();
		}
	}
}
