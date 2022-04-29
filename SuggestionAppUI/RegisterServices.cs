namespace SuggestionAppUI;

public static class RegisterServices
{
   public static void ConfigureServices(this WebApplicationBuilder builder)
   {
      // Add services to the container.
      builder.Services.AddRazorPages(); // dependency injection
      builder.Services.AddServerSideBlazor();
 
      // all relying on cache
      builder.Services.AddMemoryCache();

      builder.Services.AddSingleton<IDbConnection, DbConnection>();
      
      //Create new instance everytime 
      builder.Services.AddSingleton<ICategoryData, MongoCategoryData>(); // not unique
      builder.Services.AddSingleton<IStatusData, MongoStatusData>(); 
      builder.Services.AddSingleton<ISuggestionData, MongoSuggestionData>();
      builder.Services.AddSingleton<IUserData, MongoUserData>();
      



   }
}
