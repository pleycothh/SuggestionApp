namespace SuggestionAppLibrary.Models;

public class StatusModel
{
    [BsonId] // identifier
    [BsonRepresentation(BsonType.ObjectId)] // this is object id,
    public string Id { get; set; }
    public string StatusName { get; set; }
    public string StatusDescription { get; set; }
}