using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace PowerUp.Lego
{
    public static class LegoExtensions
    {
        public static async Task ResetAsync(this TachoMotor motor)
        {
            await motor.SetZeroAsync();
            await motor.GotoPositionAsync(0, 100, 100, SpecialSpeed.Brake);
        }


        public static async Task CycleRgbLight(this TechnicMediumHub hub, int delay)
        {
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Red);
            await Task.Delay(delay);
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Orange);
            await Task.Delay(delay);
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Yellow);
            await Task.Delay(delay);
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Green);
            await Task.Delay(delay);
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Blue);
            await Task.Delay(delay);
            await hub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Purple);
            await Task.Delay(delay);
        }

    }

}
