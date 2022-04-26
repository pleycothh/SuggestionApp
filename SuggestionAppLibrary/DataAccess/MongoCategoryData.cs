
using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess;

public class MongoCategoryData : ICategoryData
{
   private readonly IMongoCollection<CategoryModel> _categories;
   private readonly IMemoryCache _cache;
   private const string cacheName = "CategoryData";

   public MongoCategoryData(IDbConnection db, IMemoryCache cache)
   {
      _categories = db.CategoryCollection; // copy the reference of the object
      _cache = cache;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <returns></returns>
   public async Task<List<CategoryModel>> GetAllCategories()
   {
      var output = _cache.Get<List<CategoryModel>>(cacheName);

      if (output == null) //<= use cache to avoid call database every time
      {
         var result = await _categories.FindAsync(_ => true);
         output = result.ToList();

         // for more number of request at same time 
         _cache.Set(cacheName, output, TimeSpan.FromDays(1));
      }
      return output;
   }


   /// <summary>
   /// run when configure database, only run when set initial databse
   /// </summary>
   /// <param name="category"></param>
   /// <returns></returns>
   public Task CreateCategory(CategoryModel category)
   {
      return _categories.InsertOneAsync(category);
   }


}
