using Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ViteCommerce.Api.Application.GetWeatherForecast;
using ViteCommerce.Api.Application.ProductAggregate;
using ViteCommerce.Api.Application.ProductAggregate.PostProduct;
using ViteCommerce.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApiConfig(configuration);

builder.Services.ConfigureHttpJsonOptions(cfg =>
{
    //cfg.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
});

builder.Services.AddCors();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<UserDbContext>(options =>
    {
        options.UseSqlite(connectionString);
    });

builder.Services.AddIdentityServerConfig(configuration);
builder.Services.AddAuthenticationConfig(configuration);

#if DEBUG
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
#endif

builder.Services.AddDatabaseServices(configuration);
builder.Services.AddMediatorWithPipelines();
builder.Services.AddScoped<IValidator<PostProductCommand>, PostProductCommandValidator>();

var app = builder.Build();
app.Logger.LogInformation("System.Security.Cryptography.AesGcm.IsSupported: {IsSupported}", System.Security.Cryptography.AesGcm.IsSupported);
app.UseCors(opt =>
{
    opt.AllowAnyHeader();
    opt.AllowAnyMethod();
    opt.AllowAnyOrigin();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

GetWeatherForecastApi.Register(app);
ProductApi.Register(app);

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
