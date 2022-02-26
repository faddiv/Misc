using System.Text;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
});
app.Use(async (ctx, next) =>
{
    var log = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
    log.LogInformation("Request Header {0}", ctx.Request.Headers);
    await next();
    log.LogInformation("Response Header {0}", ctx.Response.Headers);
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseTus(context =>
{
    return new DefaultTusConfiguration
    {
        Store = new TusDiskStore(@"d:\Uploads\"),
        UrlPath = "/api/Upload",
        Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5)),
        Events = new Events
        {
            OnFileCompleteAsync = async ctx =>
            {
                var file = await ctx.GetFileAsync();
                var meta = await file.GetMetadataAsync(ctx.CancellationToken);
                ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                .LogInformation("File uploaded: {filename} {filetype}", meta["filename"], meta["filetype"]);
                var stream = await file.GetContentAsync(ctx.CancellationToken);
                using var newFile = File.OpenWrite(meta["filename"].GetString(Encoding.UTF8));
                await stream.CopyToAsync(newFile);
            }
        }
    };
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
