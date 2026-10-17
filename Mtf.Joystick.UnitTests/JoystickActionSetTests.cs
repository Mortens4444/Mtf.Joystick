using System;
using System.Collections.Generic;
using Xunit;

namespace Mtf.Joystick.UnitTests
{
    public class JoystickActionSetTests
    {
        private static JoystickActionSet CreateRecording(List<string> log, Action[] buttonActions = null)
        {
            return new JoystickActionSet(
                (x, y) => log.Add($"Rest {x} {y}"),
                (x, y) => log.Add($"ForwardOrBackward {x} {y}"),
                (x, y) => log.Add($"ForwardWithLeftTurn {x} {y}"),
                (x, y) => log.Add($"ForwardWithRightTurn {x} {y}"),
                (x, y) => log.Add($"BackwardWithLeftTurn {x} {y}"),
                (x, y) => log.Add($"BackwardWithRightTurn {x} {y}"),
                (x, y) => log.Add($"TurnLeftOrRight {x} {y}"),
                () => log.Add("AfterPolling"),
                buttonActions);
        }

        [Fact]
        public void Constructor_FillsEveryMissingCallbackWithANonThrowingDefault()
        {
            var actions = new JoystickActionSet(null, null, null, null, null, null, null, null, null);

            foreach (JoystickMovement movement in Enum.GetValues(typeof(JoystickMovement)))
            {
                actions.Invoke(movement, 1, 2);
            }

            actions.AfterPolling();
            actions.InvokeButtons(new[] { true, true, true, true });
            Assert.Equal(3, actions.Buttons.Length);
        }

        [Theory]
        [InlineData(nameof(JoystickMovement.Rest))]
        [InlineData(nameof(JoystickMovement.ForwardOrBackward))]
        [InlineData(nameof(JoystickMovement.ForwardWithLeftTurn))]
        [InlineData(nameof(JoystickMovement.ForwardWithRightTurn))]
        [InlineData(nameof(JoystickMovement.BackwardWithLeftTurn))]
        [InlineData(nameof(JoystickMovement.BackwardWithRightTurn))]
        [InlineData(nameof(JoystickMovement.TurnLeftOrRight))]
        public void Invoke_CallsOnlyTheActionMappedToTheMovement(string expectedName)
        {
            var log = new List<string>();
            var actions = CreateRecording(log);
            var movement = (JoystickMovement)Enum.Parse(typeof(JoystickMovement), expectedName);

            actions.Invoke(movement, 3, -4);

            Assert.Equal(new[] { $"{expectedName} 3 -4" }, log);
        }

        [Fact]
        public void Invoke_None_CallsNothing()
        {
            var log = new List<string>();
            var actions = CreateRecording(log);

            actions.Invoke(JoystickMovement.None, 3, -4);

            Assert.Empty(log);
        }

        [Fact]
        public void InvokeButtons_CallsEachPressedButtonThatHasAnAction()
        {
            var log = new List<string>();
            var actions = CreateRecording(log, new Action[] { () => log.Add("B0"), () => log.Add("B1") });

            actions.InvokeButtons(new[] { true, false, true });

            Assert.Equal(new[] { "B0" }, log);
        }

        [Fact]
        public void InvokeButtons_HandlesFewerStatesThanActions()
        {
            var log = new List<string>();
            var actions = CreateRecording(log, new Action[] { () => log.Add("B0"), () => log.Add("B1"), () => log.Add("B2") });

            actions.InvokeButtons(new[] { false, true });

            Assert.Equal(new[] { "B1" }, log);
        }

        [Fact]
        public void InvokeButtons_IgnoresNullStates()
        {
            var log = new List<string>();
            var actions = CreateRecording(log, new Action[] { () => log.Add("B0") });

            actions.InvokeButtons(null);

            Assert.Empty(log);
        }
    }
}
