using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using MilliyMock.Configurations.Layers;
using MilliyMock.Configurations;
using MilliyMock.DataAccess.Contexts;
using MilliyMock.Extensions;
using MilliyMock.Middlewares;
using MilliyMock.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.ConfigureLogger();
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

services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

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

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "Handled {RequestPath} with {StatusCode} in {Elapsed:0.0000} ms";
});

app.ApplyMigrations();
app.SeedPracticeData();

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
app.InitAccessor();
app.MapControllers();
app.UseStaticFiles();

try
{
    Log.Information("Starting application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
