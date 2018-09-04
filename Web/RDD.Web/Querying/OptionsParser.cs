using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class OptionsParser
    {
        public Options Parse(Dictionary<string, string> parameters)
        {
            var options = new Options();

            //Le filtre sur certaines opérations
            if (parameters.ContainsKey(Reserved.operations + ".id")
                || parameters.ContainsKey(Reserved.operations + ".name"))
            {
                options.FilterOperations = parameters;
            }

            //Impersonation d'un autre principal
            if (parameters.ContainsKey(Reserved.principal.ToString() + ".id"))
            {
                options.ImpersonatedPrincipal = int.Parse(parameters[Reserved.principal.ToString() + ".id"]);
            }
            
            //No Warnings
            if (parameters.ContainsKey(Reserved.nowarning.ToString()))
            {
                if (parameters[Reserved.nowarning.ToString()] == "1")
                {
                    options.WithWarnings = false;
                }
            }

            //Accept
            if (parameters.ContainsKey(Reserved.accept.ToString()))
            {
                options.Accept = parameters[Reserved.accept.ToString()];
            }

            return options;
        }
    }
}
