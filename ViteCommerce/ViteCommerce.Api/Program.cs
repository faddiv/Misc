using Database;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using ViteCommerce.Api;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(connectionString);
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes
        options.EmitStaticAudienceClaim = true;
        options.UserInteraction.LoginUrl = "/Identity/Account/Login";
        options.UserInteraction.LoginReturnUrlParameter = "ReturnUrl";
    })
    .AddApiAuthorization<IdentityUser, AppDbContext>();

builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidAudience =
            configuration.GetValue<string>("Authentication:BFF:Audience") ?? "localhost";
        opt.TokenValidationParameters.ValidIssuer = "http://localhost:3000";
        var ssk = Utilities.GetKeyFromConfig(
            configuration.GetValue<string>("Authentication:BFF:Secret") ?? "Default123");
        opt.TokenValidationParameters.TokenDecryptionKey = ssk;
        opt.TokenValidationParameters.IssuerSigningKey = ssk;
        opt.TokenValidationParameters.ValidateAudience = true;
        opt.TokenValidationParameters.ValidateIssuer = true;
        opt.TokenValidationParameters.ValidateLifetime = true;
        opt.TokenValidationParameters.ValidateIssuerSigningKey = true;
        opt.Events ??= new JwtBearerEvents();
        opt.Events.OnMessageReceived = ctx =>
        {
            return Task.CompletedTask;
        };
    })
    ;

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

#if DEBUG
IdentityModelEventSource.ShowPII = true;
#endif

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapGet("/_configuration/{clientId}", (
    [FromRoute] string clientId,
    [FromServices] IClientRequestParametersProvider clientRequestParametersProvider,
    HttpContext context) =>
{
    var parameters = clientRequestParametersProvider.GetClientParameters(context, clientId);
    return Results.Ok(parameters);
})
.WithName("GetClientParameters")
.WithOpenApi();

app.MapGet("/api/weatherforecast", (
    HttpContext ctx,
    [FromServices] ILoggerFactory logger) =>
{
    logger.CreateLogger("/api/weatherforecast").LogInformation("User: {User}", ctx.User);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization()
.WithOpenApi();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
