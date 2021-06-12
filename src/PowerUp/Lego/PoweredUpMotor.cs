using System;
using System.Threading.Tasks;
using Essentials;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpBrick.PoweredUp;

namespace PowerUp.Lego
{
    public class PoweredUpMotor
    {
        private readonly AbsoluteMotor _motor;
        private readonly ILogger _logger;

        public PoweredUpMotor(AbsoluteMotor motor, ILogger? logger = null)
        {
            _motor = motor.IsRequired();
            _logger = logger ?? NullLogger.Instance;
        }

        public bool IsConnected => _motor.IsConnected;

        public short DefaultSpeed { get; set; } = 50;
        public short DefaultPower { get; set; } = 100;

        public short Position
        {
            get
            {
                if (_motor.IsConnected)
                    return _motor.AbsolutePosition;

                throw new InvalidOperationException("Motor is not connected");
            }
        }

        public async Task SetPositionAsync(int position, int? speed = null, int? power = null)
        {
            PortFeedback feedback = await _motor.GotoPositionAsync(position,
                (sbyte)(speed ?? DefaultSpeed),
                (byte)(power ?? DefaultPower),
                SpecialSpeed.Brake);

            // TODO
            // if(andWait) while(feedback != PortFeedback.BufferEmptyAndCommandCompleted) await Task.Delay(100); // ??
        }

        public async Task SubscribePositionAsync(Action<short> callback)
        {
            //await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
            //motor.PositionObservable.Subscribe(v =>
            //{
            //    _logger.LogDebug($"Position: {v.SI}");
            //});
            //_logger.LogDebug($"Subscribe Position: {motor.Position}");

            await _motor.SetupNotificationAsync(_motor.ModeIndexAbsolutePosition, true);
            _motor.AbsolutePositionObservable.Subscribe(v =>
            {
                _logger.LogDebug($"Subscribe AbsolutePosition: {v.SI}");
                callback.Invoke(v.SI);
            });
            _logger.LogDebug($"AbsolutePosition: {_motor.AbsolutePosition}");

        }

    }
}
