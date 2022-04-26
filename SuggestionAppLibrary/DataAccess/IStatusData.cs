
namespace SuggestionAppLibrary.DataAccess;

public interface IStatusData
{
   Task CreaseStatus(StatusModel status);
   Task<List<StatusModel>> GetAllStatuses();
}