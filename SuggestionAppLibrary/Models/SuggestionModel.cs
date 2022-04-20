namespace SuggestionAppLibrary.Models;

public class SuggestionModel
{
    [BsonId] // identifier
    [BsonRepresentation(BsonType.ObjectId)] // this is object id,
    public string Id { get; set; }
    public string Suggestion { get; set; }
    public string Description { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}