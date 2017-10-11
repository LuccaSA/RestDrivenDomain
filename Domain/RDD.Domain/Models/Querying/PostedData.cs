using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using NExtends.Primitives;

namespace RDD.Domain.Models.Querying
{
	//Objet pseudo-dynamic, permet de stocker ce qui arrive du client lors d'un POST/PUT
	public class PostedData
	{
		public string name;
		public string value;
		public List<PostedData> values => subs.Values.ToList();
	    public JToken rawObject;

		//Pour un objet JSON, les subs sont les clé/valeur, pour un array JSON, les clés sont les index
		public Dictionary<string, PostedData> subs { get; private set; }

		public PostedData()
		{
			name = "this";
			value = null;
			subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
		}

		//On veut garder une trace de l'objet JSON d'origine, pour pouvoir le caster en classe C# plus tard
		public PostedData(JToken rawObject_)
			: this()
		{
			rawObject = rawObject_;
		}

		/// <summary>
		/// Value peut être soit un string, soit un string[]
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static PostedData CreateInstance(string name, object value)
		{
			if (value.GetType() == typeof(string[]))
			{
				return ParseArray(null, (string[])value);
			}
			else if (value.GetType() == typeof(int[]))
			{
				return ParseArray((int[])value);
			}
			else
			{
				return new PostedData(name, (string)value);
			}
		}
		private PostedData(string name, string value)
		{
			this.name = name;
			this.value = value;
			subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
		}
		public static PostedData Parse(string singleValue) => new PostedData("this", singleValue);

        public static PostedData ParseUrlEncoded(string data)
		{
			var dictionary = data.Split('&').ToDictionary(el => el.Split('=')[0], el => el.Split('=')[1]);

			return ParseDictionary(dictionary);
		}
		/// <summary>
		/// Quand on envoie un dico simple de string, string
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static PostedData ParseDictionary(Dictionary<string, string> data)
		{
			return ParseDictionary(data.ToDictionary(el => el.Key, el => (object)el.Value));
		}

		/// <summary>
		/// Quand on envoie un dico avec éventuellement des string[] en value
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static PostedData ParseDictionary(Dictionary<string, object> data)
		{
			var post = new PostedData();

			foreach (var element in data)
			{
				post.subs.Add(element.Key.ToLower(), CreateInstance(element.Key, element.Value));
			}

			return post;
		}
		public static PostedData ParseArray(JToken rawObject, string[] array)
		{
			var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < array.Length; i++)
			{
				subs.Add(i.ToString(), new PostedData() { value = array[i] });
			}

			return new PostedData(rawObject) { subs = subs };
		}
		public static PostedData ParseArray(int[] array)
		{
			var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < array.Length; i++)
			{
				subs.Add(i.ToString(), new PostedData() { value = array[i].ToString() });
			}

			return new PostedData() { subs = subs };
		}
		public static PostedData ParseJSONArray(JToken rawObject, JToken[] array)
		{
			if (array.Length > 0)
			{
				//Si c'est un JObject[]
				if (array[0].HasValues)
				{
					var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
					for (var i = 0; i < array.Length; i++)
					{
						subs.Add(i.ToString(), ParseJSON((JObject)array[i]));
					}

					return new PostedData(rawObject) { subs = subs };
				}
				else //JValue simple
				{
					return ParseArray(rawObject, array.Select(jToken => ((JValue)jToken).Value<string>()).ToArray());
				}
			}
			else
			{
				return new PostedData(rawObject);
			}
		}
		public static PostedData ParseJSON(string data)
		{
			var rawObject = JsonConvert.DeserializeObject(data);

			return ParseJSON((JObject)rawObject);
		}
		public static PostedData ParseJSONArray(string data)
		{
			var array = JsonConvert.DeserializeObject<JObject[]>(data);

			return ParseJSONArray(data, array);
		}
		public static PostedData ParseJSONArray(JArray array)
		{
			return ParseJSONArray(array, array.Select(el => el).ToArray());
		}
		private static PostedData ParseJSON(JObject jObject)
		{
			var post = new PostedData(jObject);

			foreach (var element in jObject)
			{
				JToken jValue = element.Value;

				//object JSON neasted ou array, attention au array vide
				if (jValue.Type == JTokenType.Object || jValue.Type == JTokenType.Array)
				{
					if (jValue.Type == JTokenType.Array)
					{
						post.subs.Add(element.Key.ToLower(), ParseJSONArray((JArray)jValue));
					}
					else //Objet neasted
					{
						post.subs.Add(element.Key.ToLower(), ParseJSON((JObject)jValue));
					}
				}
				else //Value simple
				{
					//Cas particulier pour les dates
					if (jValue.Type == JTokenType.Date)
					{
						post.subs.Add(element.Key.ToLower(), new PostedData(element.Key, ((DateTime)jValue).ToISOz()));
					}
					else
					{
						post.subs.Add(element.Key.ToLower(), new PostedData(element.Key, jValue.Value<string>()));
					}
				}
			}

			return post;
		}

		public PostedData this[string key]
		{
			get => subs[key];
		    set => subs[key] = value;
		}
		public PostedData this[int index]
		{
			get => subs[index.ToString()];
		    set => subs[index.ToString()] = value;
		}
		public PostedData this[Enum key]
		{
			get => subs[key.ToString()];
		    set => subs[key.ToString()] = value;
		}
		public bool ContainsKey(string key) => subs.ContainsKey(key);
        public bool ContainsKey(Enum key) => subs.ContainsKey(key);

        public bool Remove(string key) => subs.Remove(key);

        public ICollection<string> Keys => subs == null ? null : subs.Keys;
	    public int Count() => subs.Count;
        public bool HasSubs => subs.Count > 0;
	}
}
