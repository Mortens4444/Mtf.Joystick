using SharpDX.DirectInput;
using System.Diagnostics.CodeAnalysis;

namespace Mtf.Joystick
{
    /// <summary>
    /// <see cref="IJoystickDevice"/> backed by a real SharpDX DirectInput joystick.
    /// </summary>
    [ExcludeFromCodeCoverage] // Requires a physically attached game controller.
    internal sealed class DirectInputJoystickDevice : IJoystickDevice
    {
        private readonly SharpDX.DirectInput.Joystick joystick;

        private DirectInputJoystickDevice(SharpDX.DirectInput.Joystick joystick)
        {
            this.joystick = joystick;
        }

        /// <summary>
        /// Opens the attached game controller at <paramref name="deviceIndex"/>, or returns null when there is none.
        /// </summary>
        public static IJoystickDevice Open(int deviceIndex)
        {
            if (deviceIndex < 0)
            {
                return null;
            }

            using (var directInput = new DirectInput())
            {
                var gameControlDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
                if (gameControlDevices.Count <= deviceIndex)
                {
                    return null;
                }

                var joystick = new SharpDX.DirectInput.Joystick(directInput, gameControlDevices[deviceIndex].InstanceGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                joystick.Poll();
                return new DirectInputJoystickDevice(joystick);
            }
        }

        public void Poll()
        {
            joystick.Poll();
        }

        public JoystickState GetCurrentState()
        {
            return joystick.GetCurrentState();
        }

        public void Dispose()
        {
            joystick.Dispose();
        }
    }
}
