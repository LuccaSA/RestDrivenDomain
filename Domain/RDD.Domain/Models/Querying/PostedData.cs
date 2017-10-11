using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NExtends.Primitives;

namespace RDD.Domain.Models.Querying
{
    //Objet pseudo-dynamic, permet de stocker ce qui arrive du client lors d'un POST/PUT
    public class PostedData
    {
        public PostedData()
        {
            Name = "this";
            Value = null;
            Subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
        }

        //On veut garder une trace de l'objet JSON d'origine, pour pouvoir le caster en classe C# plus tard
        public PostedData(JToken rawObject)
            : this()
        {
            RawObject = rawObject;
        }

        private PostedData(string name, string value)
        {
            Name = name;
            Value = value;
            Subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
        }
 
        public string Name { get; set; }
        public string Value { get; set; }
        public List<PostedData> Values => Subs.Values.ToList();
        public JToken RawObject { get; set; }

        //Pour un objet JSON, les subs sont les clé/valeur, pour un array JSON, les clés sont les index
        public Dictionary<string, PostedData> Subs { get; private set; }

        public PostedData this[string key]
        {
            get => Subs[key];
            set => Subs[key] = value;
        }

        public PostedData this[int index]
        {
            get => Subs[index.ToString()];
            set => Subs[index.ToString()] = value;
        }

        public PostedData this[Enum key]
        {
            get => Subs[key.ToString()];
            set => Subs[key.ToString()] = value;
        }

        public ICollection<string> Keys => Subs?.Keys;
        public bool HasSubs => Subs.Count > 0;

        /// <summary>
        /// Value peut être soit un string, soit un string[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static PostedData CreateInstance(string name, object value)
        {
            if (value.GetType() == typeof(string[]))
            {
                return ParseArray(null, (string[]) value);
            }
            if (value.GetType() == typeof(int[]))
            {
                return ParseArray((int[]) value);
            }
            return new PostedData(name, (string) value);
        }

        public static PostedData Parse(string singleValue) => new PostedData("this", singleValue);

        public static PostedData ParseUrlEncoded(string data)
        {
            Dictionary<string, string> dictionary = data.Split('&').ToDictionary(el => el.Split('=')[0], el => el.Split('=')[1]);

            return ParseDictionary(dictionary);
        }

        /// <summary>
        /// Quand on envoie un dico simple de string, string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PostedData ParseDictionary(Dictionary<string, string> data)
        {
            return ParseDictionary(data.ToDictionary(el => el.Key, el => (object) el.Value));
        }

        /// <summary>
        /// Quand on envoie un dico avec éventuellement des string[] en value
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PostedData ParseDictionary(Dictionary<string, object> data)
        {
            var post = new PostedData();

            foreach (KeyValuePair<string, object> element in data)
            {
                post.Subs.Add(element.Key.ToLower(), CreateInstance(element.Key, element.Value));
            }

            return post;
        }

        public static PostedData ParseArray(JToken rawObject, string[] array)
        {
            var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < array.Length; i++)
            {
                subs.Add(i.ToString(), new PostedData
                {
                    Value = array[i]
                });
            }

            return new PostedData(rawObject)
            {
                Subs = subs
            };
        }

        public static PostedData ParseArray(int[] array)
        {
            var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < array.Length; i++)
            {
                subs.Add(i.ToString(), new PostedData
                {
                    Value = array[i].ToString()
                });
            }

            return new PostedData
            {
                Subs = subs
            };
        }

        public static PostedData ParseJsonArray(JToken rawObject, JToken[] array)
        {
            if (array.Length > 0)
            {
                //Si c'est un JObject[]
                if (array[0].HasValues)
                {
                    var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; i < array.Length; i++)
                    {
                        subs.Add(i.ToString(), ParseJson((JObject) array[i]));
                    }

                    return new PostedData(rawObject)
                    {
                        Subs = subs
                    };
                }
                return ParseArray(rawObject, array.Select(jToken => ((JValue) jToken).Value<string>()).ToArray());
            }
            return new PostedData(rawObject);
        }

        public static PostedData ParseJson(string data)
        {
            object rawObject = JsonConvert.DeserializeObject(data);

            return ParseJson((JObject) rawObject);
        }

        public static PostedData ParseJsonArray(string data)
        {
            var array = JsonConvert.DeserializeObject<JObject[]>(data);

            return ParseJsonArray(data, array);
        }

        public static PostedData ParseJsonArray(JArray array)
        {
            return ParseJsonArray(array, array.Select(el => el).ToArray());
        }

        private static PostedData ParseJson(JObject jObject)
        {
            var post = new PostedData(jObject);

            foreach (KeyValuePair<string, JToken> element in jObject)
            {
                JToken jValue = element.Value;

                //object JSON neasted ou array, attention au array vide
                if (jValue.Type == JTokenType.Object || jValue.Type == JTokenType.Array)
                {
                    if (jValue.Type == JTokenType.Array)
                    {
                        post.Subs.Add(element.Key.ToLower(), ParseJsonArray((JArray) jValue));
                    }
                    else //Objet neasted
                    {
                        post.Subs.Add(element.Key.ToLower(), ParseJson((JObject) jValue));
                    }
                }
                else //Value simple
                {
                    //Cas particulier pour les dates
                    if (jValue.Type == JTokenType.Date)
                    {
                        post.Subs.Add(element.Key.ToLower(), new PostedData(element.Key, ((DateTime) jValue).ToISOz()));
                    }
                    else
                    {
                        post.Subs.Add(element.Key.ToLower(), new PostedData(element.Key, jValue.Value<string>()));
                    }
                }
            }

            return post;
        }

        public bool ContainsKey(string key) => Subs.ContainsKey(key);
        public bool ContainsKey(Enum key) => Subs.ContainsKey(key);
        public bool Remove(string key) => Subs.Remove(key);
        public int Count() => Subs.Count;
    }
}