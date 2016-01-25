using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDD.Infra.Storage.MongoDB
{
	/// <summary>
	/// Interface des ressources stockées dans MongoDB
	/// </summary>
	public interface IMongoDBResource
	{
		/// <summary>
		/// Mongo C# Driver ObjectId (eg: "549400e1290cf7ff182be625")
		/// Attribut BsonId pour indiquer à MongoDB quelle clé doit être utliliser comme PK
		/// </summary>
		[BsonId]
		string ID { get; }

		/// <summary>
		/// Date CreatedOn dans l'interface pour toujours avoir accès à une clé d'ordonnancement explicite
		/// </summary>
		[BsonElement("createdOn")]
		DateTime CreatedOn { get; set; }
	}
}
