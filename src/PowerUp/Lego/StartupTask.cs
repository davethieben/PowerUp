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
        private readonly PoweredUpHub _poweredUp;
        private readonly MessageHub _messageHub;
        private readonly IOptions<PoweredUpOptions> _options;
        private readonly ILogger<StartupTask> _logger;

        public StartupTask(
            PoweredUpHub poweredUp,
            MessageHub messageHub,
            IOptions<PoweredUpOptions> options,
            ILogger<StartupTask> logger)
        {
            _poweredUp = poweredUp;
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


            // add this when you are interested in a tracing of the message ("human readable")
            if (_options.Value.EnableTracing)
            {
                await _poweredUp.EnableTracingAsync();
            }

            stopToken.ThrowIfCancellationRequested();

            await _poweredUp.ConnectAsync();

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

            await _poweredUp.SetColorAsync(PoweredUpColor.Green);

            PoweredUpMotor? motorA = await _poweredUp.GetMotorAsync(PoweredUpHub.PortA);
            if (motorA != null)
            {
                await motorA.SubscribePositionAsync(pos =>
                {
                    _logger.LogDebug($"PoweredUpMotor:A Position: {pos}");
                    _messageHub.Publish("MotorAPosition", pos);
                });

                _messageHub.Subscribe<short>("SetMotorAPosition", async (pos) =>
                {
                    await motorA.SetPositionAsync(pos);
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
                    if (motorA.IsConnected)
                    {
                        _logger.LogDebug($"_____ Goto: {pos}     (from: {motorA.Position})");
                        await motorA.SetPositionAsync(pos);
                        _logger.LogDebug($"AbsolutePosition1: {motorA.Position}");

                        await Task.Delay(waitBetween);
                        _logger.LogDebug($"AbsolutePosition2: {motorA.Position}");
                    }
                }


                _logger.LogDebug($"Startup Task Done");
            }

        }

    }
}
