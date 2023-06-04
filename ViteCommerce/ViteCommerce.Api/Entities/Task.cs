using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ViteCommerce.Api.Entities;

public class TaskItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public bool Done { get; set; }
}
