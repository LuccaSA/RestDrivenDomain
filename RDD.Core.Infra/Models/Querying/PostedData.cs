using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	//Objet pseudo-dynamic, permet de stocker ce qui arrive du client lors d'un POST/PUT
	public class PostedData
	{
		public string name;
		public string value;
		public List<PostedData> values { get { return subs.Values.ToList(); } }
		public JToken rawObject;

		//Pour un objet JSON, les subs sont les clé/valeur, pour un array JSON, les clés sont les index
		public Dictionary<string, PostedData> subs { get; private set; }

		public PostedData()
		{
			this.name = "this";
			this.value = null;
			this.subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
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
				return PostedData.ParseArray(null, (string[])value);
			}
			else if (value.GetType() == typeof(int[]))
			{
				return PostedData.ParseArray((int[])value);
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
			this.subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
		}
		public static PostedData Parse(string singleValue)
		{
			return new PostedData("this", singleValue);
		}
		public static PostedData ParseUrlEncoded(string data)
		{
			var dictionary = data.Split('&').ToDictionary(el => el.Split('=')[0], el => el.Split('=')[1]);

			return PostedData.ParseDictionary(dictionary);
		}
		/// <summary>
		/// Quand on envoie un dico simple de string, string
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static PostedData ParseDictionary(Dictionary<string, string> data)
		{
			return PostedData.ParseDictionary(data.ToDictionary(el => el.Key, el => (object)el.Value));
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
				post.subs.Add(element.Key.ToLower(), PostedData.CreateInstance(element.Key, element.Value));
			}

			return post;
		}
		public static PostedData ParseArray(JToken rawObject, string[] array)
		{
			var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < array.Count(); i++)
			{
				subs.Add(i.ToString(), new PostedData() { value = array[i] });
			}

			return new PostedData(rawObject) { subs = subs };
		}
		public static PostedData ParseArray(int[] array)
		{
			var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < array.Count(); i++)
			{
				subs.Add(i.ToString(), new PostedData() { value = array[i].ToString() });
			}

			return new PostedData() { subs = subs };
		}
		public static PostedData ParseJSONArray(JToken rawObject, JToken[] array)
		{
			if (array.Count() > 0)
			{
				//Si c'est un JObject[]
				if (array[0].HasValues)
				{
					var subs = new Dictionary<string, PostedData>(StringComparer.OrdinalIgnoreCase);
					for (var i = 0; i < array.Count(); i++)
					{
						subs.Add(i.ToString(), PostedData.ParseJSON((JObject)array[i]));
					}

					return new PostedData(rawObject) { subs = subs };
				}
				else //JValue simple
				{
					return PostedData.ParseArray(rawObject, array.Select(jToken => ((JValue)jToken).Value<string>()).ToArray());
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

			return PostedData.ParseJSON((JObject)rawObject);
		}
		public static PostedData ParseJSONArray(string data)
		{
			var array = JsonConvert.DeserializeObject<JObject[]>(data);

			return ParseJSONArray(data, array);
		}
		public static PostedData ParseJSONArray(JArray array)
		{
			return ParseJSONArray(array, array.Select(el => (JToken)el).ToArray());
		}
		private static PostedData ParseJSON(JObject jObject)
		{
			var post = new PostedData(jObject);

			foreach (var element in jObject)
			{
				JToken jValue = element.Value;

				//object JSON neasted ou array, attention au array vide
				if (jValue.HasValues || jValue.Type == JTokenType.Array)
				{
					if (jValue.Type == JTokenType.Array)
					{
						post.subs.Add(element.Key.ToLower(), PostedData.ParseJSONArray((JArray)jValue));
					}
					else //Objet neasted
					{
						post.subs.Add(element.Key.ToLower(), PostedData.ParseJSON((JObject)jValue));
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
			get { return subs[key]; }
			set { subs[key] = value; }
		}
		public PostedData this[int index]
		{
			get { return subs[index.ToString()]; }
			set { subs[index.ToString()] = value; }
		}
		public PostedData this[Enum key]
		{
			get { return subs[key.ToString()]; }
			set { subs[key.ToString()] = value; }
		}
		public bool ContainsKey(string key)
		{
			return subs.ContainsKey(key);
		}
		public bool ContainsKey(Enum key)
		{
			return subs.ContainsKey(key);
		}
		public ICollection<string> Keys { get { return subs == null ? null : subs.Keys; } }
		public int Count() { return subs.Count(); }
		public bool HasSubs { get { return subs.Count > 0; } }
	}
}
