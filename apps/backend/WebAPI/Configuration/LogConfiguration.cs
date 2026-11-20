namespace WebAPI.Configuration;

public static class LogConfiguration
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = false;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
        builder.Logging.AddDebug();
        return builder;
    }
}
