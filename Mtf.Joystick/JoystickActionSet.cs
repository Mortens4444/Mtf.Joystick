using System;
using System.Diagnostics;
using Action = System.Action;

namespace Mtf.Joystick
{
    /// <summary>
    /// The callbacks supplied to <see cref="JoystickHandler.InitializeJoystick"/>, with defaults filled in.
    /// </summary>
    internal sealed class JoystickActionSet
    {
        public JoystickActionSet(
            Action<int, int> restAction,
            Action<int, int> forwardOrBackwardAction,
            Action<int, int> forwardWithLeftTurnAction,
            Action<int, int> forwardWithRightTurnAction,
            Action<int, int> backwardWithLeftTurnAction,
            Action<int, int> backwardWithRightTurnAction,
            Action<int, int> turnLeftOrRightAction,
            Action afterPollingAction,
            Action[] buttonActions)
        {
            Rest = restAction ?? ((deltaX, deltaY) => Debug.WriteLine("Joystick at rest."));
            ForwardOrBackward = forwardOrBackwardAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Moving forward/backward: {deltaY}"));
            ForwardWithLeftTurn = forwardWithLeftTurnAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Forward with left turn: {deltaX}, {deltaY}"));
            ForwardWithRightTurn = forwardWithRightTurnAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Forward with right turn: {deltaX}, {deltaY}"));
            BackwardWithLeftTurn = backwardWithLeftTurnAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Backward with left turn: {deltaX}, {deltaY}"));
            BackwardWithRightTurn = backwardWithRightTurnAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Backward with right turn: {deltaX}, {deltaY}"));
            TurnLeftOrRight = turnLeftOrRightAction ?? ((deltaX, deltaY) => Debug.WriteLine($"Turning left/right: {deltaX}"));
            AfterPolling = afterPollingAction ?? (() => Debug.WriteLine("Polling stopped."));
            Buttons = buttonActions ?? new Action[]
            {
                () => Debug.WriteLine("Button 1 pressed."),
                () => Debug.WriteLine("Button 2 pressed."),
                () => Debug.WriteLine("Button 3 pressed."),
            };
        }

        public Action<int, int> Rest { get; }

        public Action<int, int> ForwardOrBackward { get; }

        public Action<int, int> ForwardWithLeftTurn { get; }

        public Action<int, int> ForwardWithRightTurn { get; }

        public Action<int, int> BackwardWithLeftTurn { get; }

        public Action<int, int> BackwardWithRightTurn { get; }

        public Action<int, int> TurnLeftOrRight { get; }

        public Action AfterPolling { get; }

        public Action[] Buttons { get; }

        /// <summary>
        /// Invokes the action mapped to <paramref name="movement"/>; <see cref="JoystickMovement.None"/> invokes nothing.
        /// </summary>
        public void Invoke(JoystickMovement movement, int deltaX, int deltaY)
        {
            switch (movement)
            {
                case JoystickMovement.Rest:
                    Rest(deltaX, deltaY);
                    break;
                case JoystickMovement.ForwardOrBackward:
                    ForwardOrBackward(deltaX, deltaY);
                    break;
                case JoystickMovement.ForwardWithLeftTurn:
                    ForwardWithLeftTurn(deltaX, deltaY);
                    break;
                case JoystickMovement.ForwardWithRightTurn:
                    ForwardWithRightTurn(deltaX, deltaY);
                    break;
                case JoystickMovement.BackwardWithLeftTurn:
                    BackwardWithLeftTurn(deltaX, deltaY);
                    break;
                case JoystickMovement.BackwardWithRightTurn:
                    BackwardWithRightTurn(deltaX, deltaY);
                    break;
                case JoystickMovement.TurnLeftOrRight:
                    TurnLeftOrRight(deltaX, deltaY);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Invokes the button action for every pressed button that has an action mapped.
        /// </summary>
        public void InvokeButtons(bool[] buttonStates)
        {
            if (buttonStates == null)
            {
                return;
            }

            var count = Math.Min(buttonStates.Length, Buttons.Length);
            for (var i = 0; i < count; i++)
            {
                if (buttonStates[i])
                {
                    Buttons[i]();
                }
            }
        }
    }
}
