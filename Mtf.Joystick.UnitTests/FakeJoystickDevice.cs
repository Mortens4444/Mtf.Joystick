using SharpDX.DirectInput;
using System;
using System.Collections.Generic;

namespace Mtf.Joystick.UnitTests
{
    /// <summary>
    /// Scripted <see cref="IJoystickDevice"/>: returns the queued states in order, then repeats the last one.
    /// </summary>
    internal sealed class FakeJoystickDevice : IJoystickDevice
    {
        private readonly Queue<JoystickState> states = new Queue<JoystickState>();
        private JoystickState lastState;

        public FakeJoystickDevice(params JoystickState[] scriptedStates)
        {
            foreach (var state in scriptedStates)
            {
                states.Enqueue(state);
            }
        }

        public int PollCount { get; private set; }

        public int RemainingStates => states.Count;

        public bool Disposed { get; private set; }

        public static JoystickState At(int x, int y, params bool[] buttons)
        {
            var state = new JoystickState { X = x, Y = y };
            var target = state.Buttons;
            if (target != null && target.Length >= buttons.Length)
            {
                Array.Copy(buttons, target, buttons.Length);
            }
            else
            {
                // The setter is not public in SharpDX.
                typeof(JoystickState).GetProperty(nameof(JoystickState.Buttons)).SetValue(state, buttons);
            }

            return state;
        }

        public void Poll()
        {
            PollCount++;
        }

        public JoystickState GetCurrentState()
        {
            if (states.Count > 0)
            {
                lastState = states.Dequeue();
            }

            return lastState;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
