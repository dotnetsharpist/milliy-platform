using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using MilliyMock.Configurations.Layers;
using MilliyMock.Configurations;
using MilliyMock.DataAccess.Contexts;
using MilliyMock.Extensions;
using MilliyMock.Middlewares;
using MilliyMock.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.ConfigureJwtAuth();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.ConfigureSwagger();
services.AddMemoryCache();
services.AddHttpClient();
services.AddHttpContextAccessor();
HttpContextHelper.Accessor =
#pragma warning disable ASP0000
    builder.Services.BuildServiceProvider()
#pragma warning restore ASP0000
        .GetRequiredService<IHttpContextAccessor>();

var botConfigSection = builder.Configuration.GetSection("BotConfiguration");


services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin();
            policyBuilder.AllowAnyMethod();
            policyBuilder.AllowAnyHeader();
        });
});


var cs = builder.Configuration.GetConnectionString("local");

if (string.IsNullOrWhiteSpace(cs))
    throw new Exception("Connection string is NULL");

if (!cs.Contains("Host="))
    throw new Exception("Host is missing in connection string");

Console.WriteLine(cs);
services.AddDbContext<MilliyMockDbContext>(options =>
    options.UseNpgsql(cs));

builder.ConfigureDataAccess();
builder.ConfigureServices();
var app = builder.Build();

app.ApplyMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


EnvironmentHelper.WebRootPath = Path.GetFullPath("wwwroot");

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();