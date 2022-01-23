using AspNetCore.Proxy;

var builder = WebApplication.CreateBuilder(args);
const string client = "react-server";
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddProxies();
builder.Services.AddHttpClient(client)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseProxies(proxies =>
{
    proxies.Map("static/{**rest}", proxy =>
    {
        proxy.UseHttp((ctx, args) => $"http://localhost:3000/static/{args["rest"]}", httpProxy =>
        {
            httpProxy.WithHttpClientName(client);
        });
    });
});
//

app.MapRazorPages();

app.Run();
