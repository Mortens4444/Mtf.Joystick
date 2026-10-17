using SharpDX.DirectInput;
using System;

namespace Mtf.Joystick
{
    /// <summary>
    /// Minimal view of a joystick device used by <see cref="JoystickHandler"/>.
    /// Abstracted so the polling logic can be exercised without DirectInput hardware.
    /// </summary>
    internal interface IJoystickDevice : IDisposable
    {
        void Poll();

        JoystickState GetCurrentState();
    }
}
