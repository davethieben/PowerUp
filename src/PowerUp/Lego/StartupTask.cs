using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerUp.Services;
using SharpBrick.PoweredUp;

namespace PowerUp.Lego
{
    public class StartupTask : BackgroundService
    {
        private readonly PoweredUpHost _poweredUp;
        private readonly IHostApplicationLifetime _taskLifetime;
        private readonly MessageHub _messageHub;
        private readonly IOptions<PoweredUpOptions> _options;
        private readonly ILogger<StartupTask> _logger;

        public StartupTask(
            PoweredUpHost poweredUp,
            IHostApplicationLifetime taskLifetime,
            MessageHub messageHub,
            IOptions<PoweredUpOptions> options,
            ILogger<StartupTask> logger)
        {
            _poweredUp = poweredUp;
            _taskLifetime = taskLifetime;
            _messageHub = messageHub;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            stopToken.Register(() =>
            {
                _logger.LogWarning("Cancellation Requested");
            });

            _logger.LogInformation("Waiting to discover...");

            TechnicMediumHub technicMediumHub = await _poweredUp.DiscoverAsync<TechnicMediumHub>(stopToken);
            using (var hub = new PoweredUpHub(technicMediumHub))
            {
                // add this when you are interested in a tracing of the message ("human readable")
                if (_options.Value.EnableTracing)
                {
                    await hub.EnableTracingAsync();
                }

                stopToken.ThrowIfCancellationRequested();

                await hub.ConnectAsync();

                //_logger.LogInformation($"{hub.AdvertisingName}: Port A - {hub.A.DeviceType} - attached: {hub.A.IsDeviceAttached}");
                //_logger.LogInformation($"{hub.AdvertisingName}: Port B - {hub.B.DeviceType} - attached: {hub.B.IsDeviceAttached}");
                //_logger.LogInformation($"{hub.AdvertisingName}: Port C - {hub.C.DeviceType} - attached: {hub.C.IsDeviceAttached}");
                //_logger.LogInformation($"{hub.AdvertisingName}: Port D - {hub.D.DeviceType} - attached: {hub.D.IsDeviceAttached}");

                stopToken.ThrowIfCancellationRequested();

                //await hub.TiltSensor.SetupNotificationAsync(hub.TiltSensor.ModeIndexPosition, true);
                //hub.TiltSensor.PositionObservable.Subscribe(v =>
                //{
                //    //_logger.LogDebug($"Subscribe TiltSensor.Position: {v.x}, {v.y}, {v.z}");
                //});

                await hub.SetColorAsync(PoweredUpColor.Green);

                PoweredUpMotor? motor = await hub.GetMotorAsync(PoweredUpHub.PortA);
                if (motor != null)
                {
                    await motor.SubscribePositionAsync(pos =>
                    {
                        _logger.LogDebug($"PoweredUpMotor Position: {pos}");
                        _messageHub.Publish("MotorAPosition", pos.ToString());
                    });

                    stopToken.ThrowIfCancellationRequested();

                    _logger.LogDebug($"____ Resetting");
                    var waitBetween = TimeSpan.FromMilliseconds(2000);

                    await Goto(0);

                    await Goto(20);

                    await Goto(-20);

                    await Goto(20);

                    await Goto(-20);


                    async Task Goto(int pos)
                    {
                        if (motor.IsConnected)
                        {
                            _logger.LogDebug($"_____ Goto: {pos}     (from: {motor.Position})");
                            await motor.GotoPositionAsync(pos);
                            _logger.LogDebug($"AbsolutePosition1: {motor.Position}");

                            await Task.Delay(waitBetween);
                            _logger.LogDebug($"AbsolutePosition2: {motor.Position}");
                        }
                    }

                }


                await hub.DisconnectAsync();

                _logger.LogDebug($"Startup Task Done");
            }

            _taskLifetime.StopApplication();
        }

    }
}
