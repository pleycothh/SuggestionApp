namespace SuggestionAppLibrary.Models;

public class SuggestionModel
{
   [BsonId] // identifier
   [BsonRepresentation(BsonType.ObjectId)] // this is object id,
   public string Id { get; set; }
   public string Suggestion { get; set; }
   public string Description { get; set; }
   public DateTime DateCreated { get; set; } = DateTime.UtcNow;
   public CategoryModel Category { get; set; }
   public BasicUserModel Author { get; set; } // <- need change
   public HashSet<string> UserVotes { get; set; } = new(); // unique value stirng
   public StatusModel SuggestionStatus { get; set; } // watched, completed, upComming
   public string OwnerNotes { get; set; }
   public bool ApprovedForRealease { get; set; } = false; 
   public bool Archived { get; set; } = false; // been Archived - not include when loading into view
   public bool Rejected { get; set; } = false;
}