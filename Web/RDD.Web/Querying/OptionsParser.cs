﻿using RDD.Domain;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class OptionsParser<TEntity>
        where TEntity : class
    {
        public Options Parse(Dictionary<string, string> parameters, Field fields, Field collectionFields)
        {
            var options = new Options();

            //Si les fields demandent des propriétés sur la collection
            if (collectionFields.Contains<ISelection<TEntity>>(c => c.Count))
            {
                options.NeedCount = true;

                //Et uniquement sur le count de la collection ?
                if (collectionFields.Count == 1)
                {
                    //Alors pas besoin d'énumérer les entités
                    options.NeedEnumeration = false;
                }
            }

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
