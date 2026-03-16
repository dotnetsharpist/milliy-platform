using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MilliyMock.Configurations;

public static class JwtConfiguration
{
    /// <summary>
    /// Add JWT credentials from appsettings.json and configure it
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="builder"></param>
    public static void ConfigureJwtAuth(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration.GetSection("Jwt");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["SecurityKey"]!))
                };
            });
    }
}