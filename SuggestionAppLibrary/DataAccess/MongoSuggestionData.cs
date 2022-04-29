using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess;

/// <summary>
/// permery using of data
/// </summary>
public class MongoSuggestionData : ISuggestionData
{
   private readonly IDbConnection _db;
   private readonly IUserData _userData;
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<SuggestionModel> _suggestions;

   private const string CacheName = "CategoryData";

   public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
   {
      _db = db;
      _userData = userData;
      _cache = cache;
      _suggestions = db.SuggestionCollection;
   }

   public async Task<List<SuggestionModel>> GetAllSuggestions()
   {
      var output = _cache.Get<List<SuggestionModel>>(CacheName);
      if (output is null)
      {
         var results = await _suggestions.FindAsync(s => s.Archived == false);
         output = results.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));

      }
      return output;
   }

   /// <summary>
   /// Approved suggestion Item will show on the Home Page
   /// </summary>
   /// <returns></returns>
   public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()
   {
      var output = await GetAllSuggestions();
      return output.Where(x => x.ApprovedForRealease).ToList();
   }

   public async Task<SuggestionModel> GetSuggestions(string id)
   {
      var results = await _suggestions.FindAsync(x => x.Id == id);
      return results.FirstOrDefault();
   }

   public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
   {
      var output = await GetAllSuggestions();
      return output.Where(x => x.ApprovedForRealease == false && x.Rejected == false).ToList();
   }


   /// <summary>
   /// more complex version required when user over 1000 because never hit timeout for cache
   /// </summary>
   /// <param name="suggestion"></param>
   /// <returns></returns>
   public async Task UpdateSuggestion(SuggestionModel suggestion)
   {
      await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
      _cache.Remove(CacheName); // destore suggestion data cache, since update
   }

   public async Task UpvoteSuggestion(string suggestionId, string userId)
   {
      var client = _db.Client;

      ///
      /// create transaction allow to write two different collections, fail or success together
      /// notice user to upvoate, notice service the use has upvoate
      ///
      using var session = await client.StartSessionAsync();

      session.StartTransaction();
      try
      {
         var db = client.GetDatabase(_db.DbName);
         var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
         var suggestion = (await suggestionsInTransaction.FindAsync(x => x.Id == suggestionId)).First();

         /// uservotes has hash set
         bool isUpvote = suggestion.UserVotes.Add(userId);
         if (isUpvote == false)
         {
            suggestion.UserVotes.Remove(userId);
         }

         /// repalce the new version of the (userVotes)
         await suggestionsInTransaction.ReplaceOneAsync(s => s.Id == suggestionId, suggestion);

         ///
         var usersIntransaction = db.GetCollection<UserModel>(_db.UserCollectionName);

         var user = await _userData.GetUser(suggestion.Author.Id);

         if (isUpvote)
         {
            // conver full suggestion to basic suggestion model
            user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
         }
         else
         {
            // create new instance of basic suggestion model
            var suggestionToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
            user.VotedOnSuggestions.Remove(suggestionToRemove);
         }
         await usersIntransaction.ReplaceOneAsync(u => u.Id == userId, user);

         await session.CommitTransactionAsync();

         _cache.Remove(CacheName);

      }
      catch (Exception ex)
      {
         // loging 
         await session.AbortTransactionAsync();
         throw; // throw all way up
      }
   }

   /// <summary>
   /// admain will not see update within one minite
   /// Does not need update cache because: when create suggession, that is not going to the list for user to see,
   /// User will not see the new suggestion until admain approve, hence no need to update cache
   /// </summary>
   /// <param name="suggestion"></param>
   /// <returns></returns>
   public async Task CreateSuggestion(SuggestionModel suggestion)
   {
      var client = _db.Client;
      using var session = await client.StartSessionAsync();

      session.StartTransaction(); // <-- error here
      try
      {

         var db = client.GetDatabase(_db.DbName);
         var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
         await suggestionsInTransaction.InsertOneAsync(suggestion);

         var userInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(suggestion.Author.Id);
         user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));

         /// Replace user, update user suggesstion list
         await userInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

         // since, the cache is not distroied immidelty, admain  will not be able to see update within one minute
         await session.CommitTransactionAsync();


      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
    
      }
   }
}
