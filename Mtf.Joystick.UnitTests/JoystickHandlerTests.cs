using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Mtf.Joystick.UnitTests.FakeJoystickDevice;

namespace Mtf.Joystick.UnitTests
{
    public sealed class JoystickHandlerTests : IDisposable
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        private readonly Func<int, IJoystickDevice> originalFactory;
        private readonly int originalPollInterval;

        public JoystickHandlerTests()
        {
            originalFactory = JoystickHandler.DeviceFactory;
            originalPollInterval = JoystickHandler.PollIntervalMilliseconds;
            JoystickHandler.PollIntervalMilliseconds = 1;
        }

        public void Dispose()
        {
            JoystickHandler.StopJoystick();
            JoystickHandler.DeviceFactory = originalFactory;
            JoystickHandler.PollIntervalMilliseconds = originalPollInterval;
        }

        /// <summary>
        /// Polls <paramref name="device"/> until its scripted states are used up, waits for the loop to finish, then stops.
        /// Every callback appends to <paramref name="log"/>.
        /// </summary>
        private static bool RunScript(FakeJoystickDevice device, List<string> log, Action[] buttonActions = null, int deviceIndex = 0)
        {
            var finished = new ManualResetEventSlim();
            JoystickHandler.DeviceFactory = index => index == deviceIndex ? device : null;
            var initialized = JoystickHandler.InitializeJoystick(
                getDeviceIndex: () => deviceIndex,
                continuePulling: () => device.RemainingStates > 0,
                getDeltaModifier: () => 10,
                getMinimumDelta: () => 5,
                restAction: (x, y) => log.Add($"Rest {x} {y}"),
                forwardOrBackwardAction: (x, y) => log.Add($"ForwardOrBackward {x} {y}"),
                forwardWithLeftTurnAction: (x, y) => log.Add($"ForwardWithLeftTurn {x} {y}"),
                forwardWithRightTurnAction: (x, y) => log.Add($"ForwardWithRightTurn {x} {y}"),
                backwardWithLeftTurnAction: (x, y) => log.Add($"BackwardWithLeftTurn {x} {y}"),
                backwardWithRightTurnAction: (x, y) => log.Add($"BackwardWithRightTurn {x} {y}"),
                turnLeftOrRightAction: (x, y) => log.Add($"TurnLeftOrRight {x} {y}"),
                afterPullingAction: () =>
                {
                    log.Add("AfterPolling");
                    finished.Set();
                },
                buttonActions: buttonActions);

            if (initialized)
            {
                Assert.True(finished.Wait(Timeout), "The polling loop did not finish.");
            }

            JoystickHandler.StopJoystick();
            return initialized;
        }

        [Fact]
        public void InitializeJoystick_ReturnsFalseWhenNoDeviceIsAtTheRequestedIndex()
        {
            var requestedIndexes = new List<int>();
            JoystickHandler.DeviceFactory = index =>
            {
                requestedIndexes.Add(index);
                return null;
            };

            var result = JoystickHandler.InitializeJoystick(getDeviceIndex: () => 3);

            Assert.False(result);
            Assert.Equal(new[] { 3 }, requestedIndexes);
            Assert.Equal(3, JoystickHandler.DeviceIndex);
        }

        [Fact]
        public void InitializeJoystick_DefaultsToDeviceIndexZero()
        {
            var requestedIndexes = new List<int>();
            JoystickHandler.DeviceFactory = index =>
            {
                requestedIndexes.Add(index);
                return null;
            };

            JoystickHandler.InitializeJoystick();

            Assert.Equal(new[] { 0 }, requestedIndexes);
        }

        [Fact]
        public void InitializeJoystick_WithDefaultCallbacks_PollsAndStopsCleanly()
        {
            var device = new FakeJoystickDevice(At(1000, 1000, true), At(1000, 1000, true));
            JoystickHandler.DeviceFactory = index => device;

            Assert.True(JoystickHandler.InitializeJoystick());

            JoystickHandler.StopJoystick();
            Assert.True(device.Disposed);
            Assert.True(device.PollCount >= 1);
        }

        [Fact]
        public void InitializeJoystick_CalibratesOnTheFirstStateAndDispatchesEachLaterState()
        {
            var log = new List<string>();
            var device = new FakeJoystickDevice(
                At(1000, 1000),            // used by calibration
                At(1000, 1000),            // Rest
                At(1000, 800),             // ForwardOrBackward, deltaY = 20
                At(800, 800),              // ForwardWithLeftTurn
                At(1200, 800),             // ForwardWithRightTurn
                At(800, 1200),             // BackwardWithLeftTurn
                At(1200, 1200),            // BackwardWithRightTurn
                At(1200, 1000),            // TurnLeftOrRight
                At(1040, 960));            // inside the dead zone: Rest

            Assert.True(RunScript(device, log));

            Assert.Equal(
                new[]
                {
                    "Rest 0 0",
                    "ForwardOrBackward 0 20",
                    "ForwardWithLeftTurn -20 20",
                    "ForwardWithRightTurn 20 20",
                    "BackwardWithLeftTurn -20 -20",
                    "BackwardWithRightTurn 20 -20",
                    "TurnLeftOrRight 20 0",
                    "Rest 0 0",
                    "AfterPolling",
                },
                log);
            Assert.Equal(1040, JoystickHandler.State.X);
            Assert.Equal(960, JoystickHandler.State.Y);
            Assert.True(device.Disposed);
        }

        [Fact]
        public void InitializeJoystick_InvokesButtonActionsForPressedButtons()
        {
            var log = new List<string>();
            var device = new FakeJoystickDevice(
                At(0, 0),
                At(0, 0, false, true),
                At(0, 0, true, true, true));
            var buttons = new Action[] { () => log.Add("B0"), () => log.Add("B1") };

            Assert.True(RunScript(device, log, buttons));

            Assert.Equal(new[] { "Rest 0 0", "B1", "Rest 0 0", "B0", "B1", "AfterPolling" }, log);
        }

        [Fact]
        public void InitializeJoystick_PassesTheRequestedIndexToTheFactory()
        {
            var log = new List<string>();
            var device = new FakeJoystickDevice(At(0, 0));

            Assert.True(RunScript(device, log, deviceIndex: 2));

            Assert.Equal(2, JoystickHandler.DeviceIndex);
        }

        [Fact]
        public void InitializeJoystick_StopsThePreviousDeviceBeforeOpeningANewOne()
        {
            var first = new FakeJoystickDevice(At(0, 0));
            var second = new FakeJoystickDevice(At(0, 0));
            var devices = new Queue<FakeJoystickDevice>(new[] { first, second });
            JoystickHandler.DeviceFactory = index => devices.Dequeue();

            Assert.True(JoystickHandler.InitializeJoystick());
            Assert.True(JoystickHandler.InitializeJoystick());

            Assert.True(first.Disposed);
            Assert.False(second.Disposed);
        }

        [Fact]
        public void CalibrateJoystick_ReturnsFalseWithoutAnOpenDevice()
        {
            JoystickHandler.DeviceFactory = index => null;
            JoystickHandler.InitializeJoystick();

            Assert.False(JoystickHandler.CalibrateJoystick());
        }

        [Fact]
        public void CalibrateJoystick_ReadsTheCentreFromTheCurrentDevice()
        {
            // A stale State left behind by an earlier device must not become the centre of a new one.
            var log = new List<string>();
            var first = new FakeJoystickDevice(At(0, 0), At(0, 0));
            Assert.True(RunScript(first, log));
            log.Clear();

            var second = new FakeJoystickDevice(At(5000, 5000), At(5000, 5000));
            Assert.True(RunScript(second, log));

            Assert.Equal(new[] { "Rest 0 0", "AfterPolling" }, log);
        }

        [Fact]
        public void CalibrateJoystick_RereadsTheDeviceEachTime()
        {
            var device = new FakeJoystickDevice(At(0, 0), At(300, 300));
            JoystickHandler.DeviceFactory = index => device;
            Assert.True(JoystickHandler.InitializeJoystick(continuePulling: () => false));
            Assert.Equal(0, JoystickHandler.State.X);

            Assert.True(JoystickHandler.CalibrateJoystick());

            Assert.Equal(300, JoystickHandler.State.X);
            Assert.Equal(300, JoystickHandler.State.Y);
        }

        [Fact]
        public void StopJoystick_IsSafeWithoutAnOpenDeviceAndWhenCalledTwice()
        {
            JoystickHandler.StopJoystick();
            JoystickHandler.StopJoystick();

            Assert.False(JoystickHandler.CalibrateJoystick());
        }

        [Fact]
        public async Task StopJoystick_EndsAnEndlessPollingLoopAndDisposesTheDevice()
        {
            var afterPolling = new ManualResetEventSlim();
            var device = new FakeJoystickDevice(At(0, 0));
            JoystickHandler.DeviceFactory = index => device;
            Assert.True(JoystickHandler.InitializeJoystick(afterPullingAction: afterPolling.Set));

            var stop = Task.Run(JoystickHandler.StopJoystick);
            var completed = await Task.WhenAny(stop, Task.Delay(Timeout));

            Assert.Same(stop, completed);
            Assert.True(afterPolling.IsSet);
            Assert.True(device.Disposed);
            Assert.False(JoystickHandler.CalibrateJoystick());
        }

        [Fact]
        public void StopJoystick_CalledFromAnActionDoesNotDeadlock()
        {
            var afterPolling = new ManualResetEventSlim();
            var device = new FakeJoystickDevice(At(0, 0));
            JoystickHandler.DeviceFactory = index => device;
            Assert.True(JoystickHandler.InitializeJoystick(
                restAction: (x, y) => JoystickHandler.StopJoystick(),
                afterPullingAction: afterPolling.Set));

            Assert.True(afterPolling.Wait(Timeout), "Polling did not stop.");
            Assert.True(device.Disposed);
        }

        [Fact]
        public void StopJoystick_SwallowsExceptionsThrownByCallbacks()
        {
            var device = new FakeJoystickDevice(At(0, 0));
            JoystickHandler.DeviceFactory = index => device;
            Assert.True(JoystickHandler.InitializeJoystick(restAction: (x, y) => throw new InvalidOperationException("boom")));

            JoystickHandler.StopJoystick();

            Assert.True(device.Disposed);
        }
    }
}
