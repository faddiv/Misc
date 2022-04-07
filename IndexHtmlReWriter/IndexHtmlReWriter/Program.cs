using IndexHtmlReWriter;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoins =>
{
    endpoins.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
    var fileInfo = app.Environment.WebRootFileProvider.GetFileInfo("index.html");
    using var stream = fileInfo.CreateReadStream();
    using var reader = new StreamReader(stream, true);
    var content = reader.ReadToEnd();
    content = content.Replace("<base href=\"./\"/>", "<base href=\"/\"/>");
    var data = Encoding.UTF8.GetBytes(content);
    endpoins.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new MemoryFileProvider(new MemoryFileInfo(data, "/index.html", fileInfo.LastModified))
    });
});

app.Run();
