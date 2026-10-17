namespace Mtf.Joystick
{
    /// <summary>
    /// Pure, hardware-independent calculations used by the polling loop.
    /// </summary>
    internal static class JoystickLogic
    {
        /// <summary>
        /// Converts a raw axis position into a scaled delta from the calibrated centre and applies the dead zone.
        /// </summary>
        public static void ComputeDeltas(int x, int y, int middleX, int middleY, int deltaModifier, int minimumDelta, out int deltaX, out int deltaY)
        {
            deltaX = ApplyDeadZone((x - middleX) / deltaModifier, minimumDelta);
            deltaY = ApplyDeadZone((middleY - y) / deltaModifier, minimumDelta);
        }

        /// <summary>
        /// Zeroes a delta whose magnitude is below <paramref name="minimumDelta"/>.
        /// </summary>
        public static int ApplyDeadZone(int delta, int minimumDelta)
        {
            return delta > -minimumDelta && delta < minimumDelta ? 0 : delta;
        }

        /// <summary>
        /// Maps dead-zone adjusted deltas to the movement whose action should fire.
        /// </summary>
        public static JoystickMovement Classify(int deltaX, int deltaY, int minimumDelta)
        {
            if (deltaX == 0)
            {
                return deltaY == 0 ? JoystickMovement.Rest : JoystickMovement.ForwardOrBackward;
            }

            if (deltaY > minimumDelta)
            {
                if (deltaX < -minimumDelta)
                {
                    return JoystickMovement.ForwardWithLeftTurn;
                }

                return deltaX > minimumDelta ? JoystickMovement.ForwardWithRightTurn : JoystickMovement.None;
            }

            if (deltaY < -minimumDelta)
            {
                if (deltaX < -minimumDelta)
                {
                    return JoystickMovement.BackwardWithLeftTurn;
                }

                return deltaX > minimumDelta ? JoystickMovement.BackwardWithRightTurn : JoystickMovement.None;
            }

            if (deltaY == 0 && (deltaX < -minimumDelta || deltaX > minimumDelta))
            {
                return JoystickMovement.TurnLeftOrRight;
            }

            return JoystickMovement.None;
        }
    }
}
