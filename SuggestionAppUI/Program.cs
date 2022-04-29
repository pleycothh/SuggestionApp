using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SuggestionAppUI;

var builder = WebApplication.CreateBuilder(args);



builder.ConfigureServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{ 
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

/*
   Docker Image
   Standalone package of software. That includes everything needed to run an application
   $ docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db mongo
                              docker name  mongo port          storage     image name
    docker ps <-- check docker running 
 
 */