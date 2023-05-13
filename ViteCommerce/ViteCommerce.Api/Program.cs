using Database;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using ViteCommerce.Api.Application;
using ViteCommerce.Api.Application.ProductGroup;
using ViteCommerce.Api.Application.ProductGroup.PostProduct;
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
IdentityModelEventSource.ShowPII = true;
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

app.MapGet("/api/weatherforecast", async (
    HttpContext ctx,
    [FromServices] IMediator mediator,
    [FromServices] ILoggerFactory logger) =>
{
    logger.CreateLogger("/api/weatherforecast").LogInformation("User: {User}", ctx.User);
    return await mediator.Send(new GetWeatherForecastRequest());
})
.WithName("GetWeatherForecast")
.RequireAuthorization()
.WithOpenApi();
ProductApi.Register(app);

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
