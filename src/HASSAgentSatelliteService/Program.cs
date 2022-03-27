using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace HASSAgentSatelliteService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // initialize serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(Variables.LevelSwitch)
                .WriteTo.File(Path.Combine(Variables.LogPath, $"{DateTime.Now:yyyy-MM-dd}_HASS.Agent.Satellite.Service_.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10000000,
                    retainedFileCountLimit: 10,
                    rollOnFileSizeLimit: true,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromMilliseconds(150))
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate:
                    "[{Timestamp:MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Information("[MAIN] Version: {v}", Variables.Version);

#if DEBUG
            Variables.LevelSwitch.MinimumLevel = LogEventLevel.Debug;
            Log.Debug("[MAIN] Debugging active!");
#endif

            Log.Information("[MAIN] Service started, initializing ..");

            // build and run the worker
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // configure windows service
                .UseWindowsService(options =>
                {
                    options.ServiceName = "hass.agent.satellite.service";
                })
                // configure serilog
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog();
                })
                .ConfigureServices((_, services) =>
                {
                    // bind our worker
                    services.AddHostedService<Worker>();
                }).UseSerilog();
    }
}
