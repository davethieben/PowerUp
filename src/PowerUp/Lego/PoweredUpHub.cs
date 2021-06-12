using System;
using System.Threading.Tasks;
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

        private readonly PoweredUpHost _host;
        private TechnicMediumHub? _hub;

        public PoweredUpHub(PoweredUpHost host)
        {
            _host = host;
        }

        public async Task EnableTracingAsync()
        {
            TraceMessages tracer = _host.ServiceProvider.GetRequiredService<TraceMessages>();
            await tracer.ExecuteAsync();
        }

        public async Task<TechnicMediumHub> ConnectAsync()
        {
            if (_hub == null)
            {
                _hub = await _host.DiscoverAsync<TechnicMediumHub>();
                if (_hub.PrimaryMacAddress == null)
                {
                    await _hub.ConnectAsync();
                }

                if (!_hub.IsConnected)
                    throw new ApplicationException("Could not connect");
            }

            return _hub;
        }

        public async Task DisconnectAsync()
        {
            if (_hub != null)
            {
                await _hub.DisconnectAsync();
                _hub.Dispose();
                _hub = null;
            }
        }

        public async Task SetColorAsync(PoweredUpColor color)
        {
            _hub = await ConnectAsync();

            await _hub.RgbLight.SetRgbColorNoAsync(color);
        }

        public async Task<PoweredUpMotor?> GetMotorAsync(short port)
        {
            _hub = await ConnectAsync();

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
