using SharpDX.DirectInput;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using Action = System.Action;

namespace Mtf.Joystick
{
    public static class JoystickHandler
    {
        private static Task pollingTask;
        private static CancellationTokenSource joystickPollCancellationTokenSource;
        private static SharpDX.DirectInput.Joystick joystick;
        public static int DeviceIndex { get; private set; }
        public static JoystickState State { get; private set; }

        private static int middleX, middleY;

        public static bool InitializeJoystick(
            Func<int> getDeviceIndex = null,
            Func<bool> continuePulling = null,
            Func<int> getDeltaModifier = null,
            Func<int> getMinimumDelta = null,

            Action<int, int> restAction = null,
            Action<int, int> forwardOrBackwardAction = null,
            Action<int, int> forwardWithLeftTurnAction = null,
            Action<int, int> forwardWithRightTurnAction = null,
            Action<int, int> backwardWithLeftTurnAction = null,
            Action<int, int> backwardWithRightTurnAction = null,
            Action<int, int> turnLeftOrRightAction = null,

            Action afterPullingAction = null,

            Action[] buttonActions = null)
        {
            if (joystickPollCancellationTokenSource != null && !joystickPollCancellationTokenSource.IsCancellationRequested)
            {
                StopJoystick();
            }

            if (getDeviceIndex == null) getDeviceIndex = () => 0;
            if (continuePulling == null) continuePulling = () => true;
            if (getDeltaModifier == null) getDeltaModifier = () => 10;
            if (getMinimumDelta == null) getMinimumDelta = () => 5;

            if (restAction == null) restAction = (deltaX, deltaY) => Debug.WriteLine("Joystick at rest.");
            if (forwardOrBackwardAction == null) forwardOrBackwardAction = (deltaX, deltaY) => Debug.WriteLine($"Moving forward/backward: {deltaY}");
            if (forwardWithLeftTurnAction == null) forwardWithLeftTurnAction = (deltaX, deltaY) => Debug.WriteLine($"Forward with left turn: {deltaX}, {deltaY}");
            if (forwardWithRightTurnAction == null) forwardWithRightTurnAction = (deltaX, deltaY) => Debug.WriteLine($"Forward with right turn: {deltaX}, {deltaY}");
            if (backwardWithLeftTurnAction == null) backwardWithLeftTurnAction = (deltaX, deltaY) => Debug.WriteLine($"Backward with left turn: {deltaX}, {deltaY}");
            if (backwardWithRightTurnAction == null) backwardWithRightTurnAction = (deltaX, deltaY) => Debug.WriteLine($"Backward with right turn: {deltaX}, {deltaY}");
            if (turnLeftOrRightAction == null) turnLeftOrRightAction = (deltaX, deltaY) => Debug.WriteLine($"Turning left/right: {deltaX}");

            if (afterPullingAction == null) afterPullingAction = () => Debug.WriteLine("Polling stopped.");
            if (buttonActions == null)
            {
                buttonActions = new Action[]
                {
                    () => Debug.WriteLine("Button 1 pressed."),
                    () => Debug.WriteLine("Button 2 pressed."),
                    () => Debug.WriteLine("Button 3 pressed."),
                };
            }

            using (var directInput = new DirectInput())
            {
                var gameControlDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
                DeviceIndex = getDeviceIndex();
                if (gameControlDevices.Count > DeviceIndex)
                {
                    var deviceInstance = gameControlDevices[DeviceIndex];
                    joystick = new SharpDX.DirectInput.Joystick(directInput, deviceInstance.InstanceGuid);
                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();
                    joystick.Poll();

                    CalibrateJoystick();

                    joystickPollCancellationTokenSource = new CancellationTokenSource();
                    pollingTask = Task.Run(() =>
                    {
                        while (continuePulling() && !joystickPollCancellationTokenSource.IsCancellationRequested)
                        {
                            var deltaModifier = getDeltaModifier();
                            var minimumDelta = getMinimumDelta();
                            joystick.Poll();
                            State = joystick.GetCurrentState();
                            var deltaX = (State.X - middleX) / deltaModifier;
                            var deltaY = (middleY - State.Y) / deltaModifier;

                            if ((deltaX > -minimumDelta) && (deltaX < minimumDelta))
                            {
                                deltaX = 0;
                            }
                            if ((deltaY > -minimumDelta) && (deltaY < minimumDelta))
                            {
                                deltaY = 0;
                            }

                            if (deltaX == 0)
                            {
                                if (deltaY == 0)
                                {
                                    restAction(deltaX, deltaY);
                                }
                                else
                                {
                                    forwardOrBackwardAction(deltaX, deltaY);
                                }
                            }
                            else if (deltaY > minimumDelta)
                            {
                                if (deltaX < -minimumDelta)
                                {
                                    forwardWithLeftTurnAction(deltaX, deltaY);
                                }
                                else if (deltaX > minimumDelta)
                                {
                                    forwardWithRightTurnAction(deltaX, deltaY);
                                }
                            }
                            else if (deltaY < -minimumDelta)
                            {
                                if (deltaX < -minimumDelta)
                                {
                                    backwardWithLeftTurnAction(deltaX, deltaY);
                                }
                                else if (deltaX > minimumDelta)
                                {
                                    backwardWithRightTurnAction(deltaX, deltaY);
                                }
                            }
                            else if (deltaY == 0)
                            {
                                if (deltaX < -minimumDelta || deltaX > minimumDelta)
                                {
                                    turnLeftOrRightAction(deltaX, deltaY);
                                }
                            }

                            var buttonStates = State.Buttons;
                            for (var i = 0; i < buttonStates.Length; i++)
                            {
                                if (buttonStates[i] && buttonActions.Length > i)
                                {
                                    buttonActions[i]();
                                }
                            }

                            Thread.Sleep(100);
                        }

                        afterPullingAction();
                    }, joystickPollCancellationTokenSource.Token);
                    return true;
                }
            }

            joystickPollCancellationTokenSource = null;
            return false;
        }

        public static bool CalibrateJoystick()
        {
            if (joystick != null)
            {
                if (State == null)
                {
                    State = joystick.GetCurrentState();
                }
                middleX = State.X;
                middleY = State.Y;
                return true;
            }
            return false;
        }

        public static void StopJoystick()
        {
            joystickPollCancellationTokenSource?.Cancel();
            joystick?.Dispose();
            try
            {
                pollingTask?.Wait();
            }
            catch
            {
            }
        }
    }
}
