using MilliyMock.Service.Interfaces;
using MilliyMock.Service.Mappers;
using MilliyMock.Service.Services;

namespace MilliyMock.Configurations.Layers;

public static class ServiceLayerConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        var config = builder.Configuration.GetSection("AutoMapperLicenceKey");
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = config.Value;
            cfg.AddMaps(typeof(MapperProfile));
        });

    }
}