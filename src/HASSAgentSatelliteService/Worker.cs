using HASSAgent.Shared;
using HASSAgentSatelliteService.Commands;
using HASSAgentSatelliteService.Functions;
using HASSAgentSatelliteService.Managers;
using HASSAgentSatelliteService.RPC;
using HASSAgentSatelliteService.Sensors;
using HASSAgentSatelliteService.Settings;
using Serilog;

namespace HASSAgentSatelliteService
{
    public class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<Worker> _log;

        public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _log = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                Shutdown();
            });

            _log.LogDebug("[WORKER] StartAsync called");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _log.LogInformation("[WORKER] Startup completed, commencing execution ..");

                // load stored settings (if any)
                var launched = await SettingsManager.LoadAsync();
                if (!launched)
                {
                    _log.LogError("[WORKER] Something went wrong while loading stored configuration! Stopping ..");
                    return;
                }
                
                // initialize hass.agent shared library
                AgentSharedBase.Initialize(Variables.ServiceSettings!.DeviceName, Variables.MqttManager, Variables.ServiceSettings.CustomExecutorBinary);

                // store our startup path
                SettingsManager.StoreInstallPath();

                // initialize the RPC server
                _ = Task.Run(RpcManager.Initialize, stoppingToken);
                
                // initialize the mqtt manager
                _ = Task.Run(Variables.MqttManager.Initialize, stoppingToken);

                // initialize the sensors manager
                _ = Task.Run(SensorsManager.Initialize, stoppingToken);

                // initialize the commands manager
                _ = Task.Run(CommandsManager.Initialize, stoppingToken);
                
                // initialize the systemstate manager
                _ = Task.Run(SystemStateManager.Initialize, stoppingToken);
                
                // loop forever
                while (!stoppingToken.IsCancellationRequested)
                {
                    // check if we're asked to shutdown
                    if (Variables.CommenceShutdown) break;

                    // wait a bit
                    await Task.Delay(500, stoppingToken);
                }

                // cancelled or halted internally?
                _log.LogInformation(stoppingToken.IsCancellationRequested
                    ? "[WORKER] Cancellation requested, stopping .."
                    : "[WORKER] Halting ..");
            }
            catch (TaskCanceledException)
            {
                _log.LogWarning("[WORKER] Application closed down due to cancellation request");
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "[WORKER] Critical error: {err}", ex.Message);
            }
            finally
            {
                // stop the application
                _hostApplicationLifetime.StopApplication();

                // flush serilog
                Log.CloseAndFlush();
            }
        }

        private bool Shutdown()
        {
            _log.LogDebug("[WORKER] Shutdown called");
            HelperFunctions.ShutdownAsync().GetAwaiter().GetResult();
            return true;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("[WORKER] Stop called, halting ..");
            return base.StopAsync(cancellationToken);
        }
    }
}