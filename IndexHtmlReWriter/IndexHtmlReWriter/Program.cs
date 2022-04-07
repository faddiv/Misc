using IndexHtmlReWriter;

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

    endpoins.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new RewriterFileProvider(app.Environment.WebRootFileProvider, src => src.Replace("<base href=\"./\"/>", "<base href=\"/\"/>"))
    });
});

app.Run();