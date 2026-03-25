using Serilog;

namespace MilliyMock.Configurations;

public static class LoggingExtensions
{
    public static void ConfigureLogger(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);

            if (context.HostingEnvironment.IsProduction())
            {
                configuration.WriteTo.Console();
            }
            else
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
            }
        });
    }
}