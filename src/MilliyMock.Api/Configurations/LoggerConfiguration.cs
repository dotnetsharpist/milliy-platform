using Serilog;

namespace MilliyMock.Configurations;

public static class LoggerConfiguration
{
    public static void ConfigureLogger(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }
}