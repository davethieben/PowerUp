using System;
using System.Threading.Tasks;
using Essentials;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;

namespace PowerUp.Lego
{
    public class PoweredUpHub : IDisposable
    {
        public static readonly short PortA = 0;
        public static readonly short PortB = 1;
        public static readonly short PortC = 2;
        public static readonly short PortD = 3;

        private readonly TechnicMediumHub _hub;

        public PoweredUpHub(TechnicMediumHub hub)
        {
            _hub = hub.IsRequired();
        }

        public async Task EnableTracingAsync()
        {
            TraceMessages tracer = _hub.ServiceProvider.GetRequiredService<TraceMessages>();
            await tracer.ExecuteAsync();
        }

        public async Task ConnectAsync()
        {
            if (_hub.PrimaryMacAddress == null)
            {
                await _hub.ConnectAsync();

                if (!_hub.IsConnected)
                    throw new ApplicationException("Could not connect");
            }
        }

        public async Task DisconnectAsync()
        {
            await _hub.DisconnectAsync();
        }

        public async Task SetColorAsync(PoweredUpColor color)
        {
            await ConnectAsync();

            await _hub.RgbLight.SetRgbColorNoAsync(color);
        }

        public async Task<PoweredUpMotor?> GetMotorAsync(short port)
        {
            await ConnectAsync();

            //var motor = _hub.A.GetDevice<TechnicXLargeLinearMotor>();
            var device = _hub.Port((byte)port).GetDevice<IPoweredUpDevice>();

            if (device == null)
                return null;

            if (device is AbsoluteMotor motor)
                return new PoweredUpMotor(motor);

            throw new InvalidOperationException($"Device on port {port} is not a motor. Device: {device}");
        }

        public void Dispose()
        {
            _hub?.Dispose();
        }
    }
}
