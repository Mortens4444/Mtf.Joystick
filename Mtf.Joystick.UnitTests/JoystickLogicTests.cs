using Xunit;

namespace Mtf.Joystick.UnitTests
{
    public class JoystickLogicTests
    {
        [Theory]
        [InlineData(0, 5, 0)]
        [InlineData(4, 5, 0)]
        [InlineData(-4, 5, 0)]
        [InlineData(5, 5, 5)]
        [InlineData(-5, 5, -5)]
        [InlineData(12, 5, 12)]
        public void ApplyDeadZone_ZeroesOnlyValuesStrictlyInsideTheDeadZone(int delta, int minimumDelta, int expected)
        {
            Assert.Equal(expected, JoystickLogic.ApplyDeadZone(delta, minimumDelta));
        }

        [Fact]
        public void ComputeDeltas_ScalesByModifierAndInvertsTheYAxis()
        {
            JoystickLogic.ComputeDeltas(x: 1200, y: 300, middleX: 1000, middleY: 1000, deltaModifier: 10, minimumDelta: 5, out var deltaX, out var deltaY);

            Assert.Equal(20, deltaX);
            Assert.Equal(70, deltaY);
        }

        [Fact]
        public void ComputeDeltas_UsesIntegerDivision()
        {
            JoystickLogic.ComputeDeltas(x: 1059, y: 941, middleX: 1000, middleY: 1000, deltaModifier: 10, minimumDelta: 1, out var deltaX, out var deltaY);

            Assert.Equal(5, deltaX);
            Assert.Equal(5, deltaY);
        }

        [Fact]
        public void ComputeDeltas_AppliesTheDeadZoneToBothAxes()
        {
            JoystickLogic.ComputeDeltas(x: 1040, y: 960, middleX: 1000, middleY: 1000, deltaModifier: 10, minimumDelta: 5, out var deltaX, out var deltaY);

            Assert.Equal(0, deltaX);
            Assert.Equal(0, deltaY);
        }

        [Theory]
        [InlineData(0, 0, nameof(JoystickMovement.Rest))]
        [InlineData(0, 8, nameof(JoystickMovement.ForwardOrBackward))]
        [InlineData(0, -8, nameof(JoystickMovement.ForwardOrBackward))]
        [InlineData(-8, 8, nameof(JoystickMovement.ForwardWithLeftTurn))]
        [InlineData(8, 8, nameof(JoystickMovement.ForwardWithRightTurn))]
        [InlineData(-8, -8, nameof(JoystickMovement.BackwardWithLeftTurn))]
        [InlineData(8, -8, nameof(JoystickMovement.BackwardWithRightTurn))]
        [InlineData(-8, 0, nameof(JoystickMovement.TurnLeftOrRight))]
        [InlineData(8, 0, nameof(JoystickMovement.TurnLeftOrRight))]
        public void Classify_MapsEachQuadrantToItsMovement(int deltaX, int deltaY, string expected)
        {
            Assert.Equal(expected, JoystickLogic.Classify(deltaX, deltaY, minimumDelta: 5).ToString());
        }

        [Theory]
        [InlineData(5, 8)]   // deltaX exactly at the threshold while moving forward
        [InlineData(-5, 8)]
        [InlineData(5, -8)]  // deltaX exactly at the threshold while moving backward
        [InlineData(-5, -8)]
        [InlineData(5, 0)]   // deltaX exactly at the threshold while level
        [InlineData(8, 5)]   // deltaY exactly at the threshold
        [InlineData(8, -5)]
        public void Classify_ReturnsNoneOnThresholdBoundaries(int deltaX, int deltaY)
        {
            Assert.Equal(JoystickMovement.None, JoystickLogic.Classify(deltaX, deltaY, minimumDelta: 5));
        }
    }
}
