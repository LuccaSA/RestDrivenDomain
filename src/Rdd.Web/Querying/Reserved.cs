using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rdd.Web.Querying
{
    public static class Reserved
    {
        [Description("Champ utilisé par jQuery")]
        public const string JQuery = "_";

        [Description("Champ utilisé pour le cache")]
        public const string Randomnumber = "randomnumber";

        [Description("Champ utilisé pour l'authentification")]
        public const string AuthToken = "authToken";

        [Description("Champ utilisé pour l'appel de fonction callback")]
        public const string Callback = "callback";

        [Description("Champ utilisé pour une demande explicite de fields")]
        public const string Fields = "fields";

        [Description("Champ utilisé pour faire des groupby")]
        public const string Groupby = "groupby";

        [Description("Champ utilisé pour les post?httpmethod=PUT")]
        public const string Httpmethod = "httpmethod";

        [Description("Champ utilisé pour l'authentification")]
        public const string Longtoken = "longtoken";

        [Description("Champ utilisé en cas de besoin d'aide exemple : ?metadata")]
        public const string Metadata = "metadata";

        [Description("Permet de bloquer la notification par mail lors d'un appel à l'API : &notify=false")]
        public const string Notify = "notify";

        [Description("Champ utilisé pour récupérer les données dans un autre format: &accept=application/xls")]
        public const string Accept = "accept";

        [Description("Champ utilisé en cas de besoin d'odonner les réultats")]
        public const string Orderby = "orderby";

        [Description("Champ utilisé en cas de besoin de paging")]
        public const string Paging = "paging";

        [Description("Champ utilisé pour montrer le template -> ne renvoie rien pour le moment")]
        public const string Template = "template";

        public static HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            JQuery,
            Randomnumber,
            AuthToken,
            Callback,
            Fields,
            Groupby,
            Httpmethod,
            Longtoken,
            Metadata,
            Notify,
            Accept,
            Orderby,
            Paging,
            Template
        };
    }
}
