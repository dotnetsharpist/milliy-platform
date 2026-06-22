using MilliyMock.Models;
using MilliyMock.Service.Handlers;
using MilliyMock.Service.Interfaces;
using MilliyMock.Service.Mappers;
using MilliyMock.Service.Services;
using Telegram.Bot;
using Telegram.Bot.AspNetCore;
using Telegram.Bot.Polling;

namespace MilliyMock.Configurations.Layers;

public static class ServiceLayerConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
        builder.Services.Configure<BotConfiguration>(botConfigSection);
        
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITempUserService, TempUserService>();
        builder.Services.AddScoped<ITestService, TestService>();
        builder.Services.AddScoped<IQuestionGroupService, QuestionGroupService>();
        builder.Services.AddScoped<IQuestionService, QuestionService>();
        builder.Services.AddScoped<IOptionService, OptionService>();
        builder.Services.AddScoped<IUserTestAttemptService, UserTestAttemptService>();
        builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
        builder.Services.AddScoped<IBotUserService, BotUserService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IQuestionExplanationService, QuestionExplanationService>();
        builder.Services.AddScoped<IUserBalanceService, UserBalanceService>();
        builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();
        builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
            httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));
        builder.Services.ConfigureTelegramBotMvc();
        
        var config = builder.Configuration.GetSection("AutoMapperLicenceKey");
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = config.Value;
            cfg.AddMaps(typeof(MapperProfile));
        });

    }
}