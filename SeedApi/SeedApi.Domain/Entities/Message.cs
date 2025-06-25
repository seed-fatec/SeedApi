using MongoDB.Bson.Serialization.Attributes;
using System;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace SeedApi.Domain.Entities
{
  public class Message
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [JsonPropertyName("sender_id")]
    public int SenderId { get; set; }

    [JsonPropertyName("recipient_id")]
    public int RecipientId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime Timestamp { get; set; }
  }
}
