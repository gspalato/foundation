using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Foundation.SDK.Database.Mongo
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
    }
}