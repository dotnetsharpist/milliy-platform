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
        builder.Services.AddScoped<ITestService, TestService>();
        builder.Services.AddScoped<IQuestionGroupService, QuestionGroupService>();
        builder.Services.AddScoped<IQuestionService, QuestionService>();
        builder.Services.AddScoped<IOptionService, OptionService>();
        builder.Services.AddScoped<UserTestAttemptService, UserTestAttemptService>();
        builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
        builder.Services.AddScoped<IFileService, FileService>();
        var config = builder.Configuration.GetSection("AutoMapperLicenceKey");
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = config.Value;
            cfg.AddMaps(typeof(MapperProfile));
        });

    }
}