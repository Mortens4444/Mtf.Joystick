using SharpDX.DirectInput;
using System;
using System.Threading;
using System.Threading.Tasks;
using Action = System.Action;

namespace Mtf.Joystick
{
    public static class JoystickHandler
    {
        private static Task pollingTask;
        private static CancellationTokenSource joystickPollCancellationTokenSource;
        private static IJoystickDevice joystick;
        private static int middleX, middleY;

        /// <summary>
        /// Opens the device at a given index, or returns null when there is none. Replaceable for testing.
        /// </summary>
        internal static Func<int, IJoystickDevice> DeviceFactory { get; set; } = DirectInputJoystickDevice.Open;

        /// <summary>
        /// Delay between two polls of the device.
        /// </summary>
        internal static int PollIntervalMilliseconds { get; set; } = 100;

        public static int DeviceIndex { get; private set; }

        public static JoystickState State { get; private set; }

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

            var actions = new JoystickActionSet(
                restAction,
                forwardOrBackwardAction,
                forwardWithLeftTurnAction,
                forwardWithRightTurnAction,
                backwardWithLeftTurnAction,
                backwardWithRightTurnAction,
                turnLeftOrRightAction,
                afterPullingAction,
                buttonActions);

            DeviceIndex = getDeviceIndex();
            var device = DeviceFactory(DeviceIndex);
            if (device == null)
            {
                joystickPollCancellationTokenSource = null;
                return false;
            }

            joystick = device;
            CalibrateJoystick();

            var cancellationTokenSource = new CancellationTokenSource();
            joystickPollCancellationTokenSource = cancellationTokenSource;
            pollingTask = Task.Run(
                () => PollLoop(device, continuePulling, getDeltaModifier, getMinimumDelta, actions, cancellationTokenSource.Token),
                cancellationTokenSource.Token);
            return true;
        }

        /// <summary>
        /// Reads the current position of the open device and uses it as the centre for subsequent delta calculations.
        /// </summary>
        public static bool CalibrateJoystick()
        {
            var device = joystick;
            if (device == null)
            {
                return false;
            }

            device.Poll();
            var state = device.GetCurrentState();
            State = state;
            middleX = state.X;
            middleY = state.Y;
            return true;
        }

        public static void StopJoystick()
        {
            joystickPollCancellationTokenSource?.Cancel();

            var task = pollingTask;
            if (task != null && task.Id != Task.CurrentId)
            {
                try
                {
                    task.Wait();
                }
                catch (AggregateException)
                {
                    // The task was cancelled before it started, or a callback threw; either way polling has ended.
                }

                pollingTask = null;
            }

            joystick?.Dispose();
            joystick = null;
        }

        private static void PollLoop(
            IJoystickDevice device,
            Func<bool> continuePulling,
            Func<int> getDeltaModifier,
            Func<int> getMinimumDelta,
            JoystickActionSet actions,
            CancellationToken cancellationToken)
        {
            while (continuePulling() && !cancellationToken.IsCancellationRequested)
            {
                var deltaModifier = getDeltaModifier();
                var minimumDelta = getMinimumDelta();
                device.Poll();
                var state = device.GetCurrentState();
                State = state;

                JoystickLogic.ComputeDeltas(state.X, state.Y, middleX, middleY, deltaModifier, minimumDelta, out var deltaX, out var deltaY);
                actions.Invoke(JoystickLogic.Classify(deltaX, deltaY, minimumDelta), deltaX, deltaY);
                actions.InvokeButtons(state.Buttons);

                Thread.Sleep(PollIntervalMilliseconds);
            }

            actions.AfterPolling();
        }
    }
}
