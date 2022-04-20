namespace SuggestionAppLibrary.Models;
public class CategoryModel
{
   [BsonId] // identifier
   [BsonRepresentation(BsonType.ObjectId)] // this is object id,
   public string Id { get; set; }
   public string CategoryName { get; set; }
   public string CategoryDescription { get; set; }

}