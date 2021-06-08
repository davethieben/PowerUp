using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace PowerUp.Lego
{
    public class StartupTask : BackgroundService
    {
        private readonly PoweredUpHost _poweredUp;
        private readonly ILogger<StartupTask> _logger;

        public StartupTask(PoweredUpHost poweredUp,  ILogger<StartupTask> logger)
        {
            _poweredUp = poweredUp;
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

                await hub.CycleRgbLight(200);


                var motor = hub.A.GetDevice<TechnicXLargeLinearMotor>();


                _logger.LogDebug($"Resetting");
                await motor.ResetAsync();
                await Task.Delay(2000);

                _logger.LogDebug($"Goto 90");
                await motor.GotoPositionAsync(90, 50, 100, SpecialSpeed.Brake);
                await Task.Delay(2000);

                _logger.LogDebug($"Goto -90");
                await motor.GotoPositionAsync(-90, 50, 100, SpecialSpeed.Brake);
                await Task.Delay(2000);

                //var input = Console.ReadLine();
                //while (input != "x")
                //{
                //    if (Int32.TryParse(input, out int position))
                //    {
                //        _logger.LogDebug($"Goto: {position}");
                //        await motor.GotoPositionAsync(position, 50, 100, SpecialSpeed.Hold);

                //    }


                //    input = Console.ReadLine();
                //}


                _logger.LogDebug($"Resetting");
                await motor.ResetAsync();
                await Task.Delay(1000);

                await hub.SwitchOffAsync();
            }
        }

    }
}
