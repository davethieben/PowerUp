using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;

namespace PowerUp.Lego
{
    public class StartupTask : BackgroundService
    {
        private readonly PoweredUpHost _poweredUp;
        private readonly IHostApplicationLifetime _taskLifetime;
        private readonly ILogger<StartupTask> _logger;

        public StartupTask(PoweredUpHost poweredUp, IHostApplicationLifetime taskLifetime, ILogger<StartupTask> logger)
        {
            _poweredUp = poweredUp;
            _taskLifetime = taskLifetime;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            stopToken.Register(() =>
            {
                _logger.LogWarning("Cancellation Requested");
            });

            _logger.LogInformation("Waiting to discover...");

            using (TechnicMediumHub hub = await _poweredUp.DiscoverAsync<TechnicMediumHub>(stopToken))
            {
                // add this when you are interested in a tracing of the message ("human readable")
                if (true)
                {
                    var tracer = hub.ServiceProvider.GetService<TraceMessages>();
                    await tracer.ExecuteAsync();
                }

                if (stopToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancelling");
                    return;
                }

                await hub.ConnectAsync();

                if (!hub.IsConnected)
                    throw new ApplicationException("Could not connect");

                _logger.LogInformation($"{hub.AdvertisingName}: Port A - {hub.A.DeviceType} - attached: {hub.A.IsDeviceAttached}");
                _logger.LogInformation($"{hub.AdvertisingName}: Port B - {hub.B.DeviceType} - attached: {hub.B.IsDeviceAttached}");
                _logger.LogInformation($"{hub.AdvertisingName}: Port C - {hub.C.DeviceType} - attached: {hub.C.IsDeviceAttached}");
                _logger.LogInformation($"{hub.AdvertisingName}: Port D - {hub.D.DeviceType} - attached: {hub.D.IsDeviceAttached}");

                await hub.TiltSensor.SetupNotificationAsync(hub.TiltSensor.ModeIndexPosition, true);
                hub.TiltSensor.PositionObservable.Subscribe(v =>
                {
                    //_logger.LogDebug($"Subscribe TiltSensor.Position: {v.x}, {v.y}, {v.z}");
                });

                //if (hub.TiltSensor.Position != null)
                //{
                //    _logger.LogDebug($"TiltSensor.Position: {hub.TiltSensor.Position.x}, {hub.TiltSensor.Position.y}, {hub.TiltSensor.Position.z}");
                //}


                await hub.CycleRgbLight(100);

                var motor = hub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
                motor.PositionObservable.Subscribe(v =>
                {
                    _logger.LogDebug($"Position: {v.SI}");
                });
                _logger.LogDebug($"Subscribe Position: {motor.Position}");

                await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);
                motor.AbsolutePositionObservable.Subscribe(v =>
                {
                    _logger.LogDebug($"Subscribe AbsolutePosition: {v.SI}");
                });
                _logger.LogDebug($"AbsolutePosition: {motor.AbsolutePosition}");


                _logger.LogDebug($"____ Resetting");
                var waitBetween = TimeSpan.FromMilliseconds(2000);

                await Goto(0);

                await Goto(20);

                await Goto(-20);

                await Goto(20);

                await Goto(-20);


                async Task Goto(int pos)
                {
                    _logger.LogDebug($"_____ Goto: {pos}     (from: {motor.AbsolutePosition})");
                    await motor.GotoPositionAsync(pos, 20, 100, SpecialSpeed.Brake);
                    _logger.LogDebug($"AbsolutePosition1: {motor.AbsolutePosition}");

                    await Task.Delay(waitBetween);
                    _logger.LogDebug($"AbsolutePosition2: {motor.AbsolutePosition}");
                }



                await hub.DisconnectAsync();

                _logger.LogDebug($"Startup Task Done");
            }

            _taskLifetime.StopApplication();
        }

    }
}
