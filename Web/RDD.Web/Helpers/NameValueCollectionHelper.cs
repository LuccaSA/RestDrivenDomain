using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rdd.Web.Helpers
{
    public class NameValueCollectionHelper
    {
        //from NExtends : https://github.com/LuccaSA/NExtends/commit/7f7c45bf5696d17cfb01c33274dcdb946586648e
        //waiting for NExtends to support this extension again
        public Dictionary<string, string> ToDictionary(NameValueCollection collection)
        {
            Dictionary<string, string> dico = new Dictionary<string, string>();

            foreach (string key in collection.AllKeys)
            {
                dico.Add(key, collection[key]);
            }

            return dico;
        }
    }
}
