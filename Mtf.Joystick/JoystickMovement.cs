namespace Mtf.Joystick
{
    /// <summary>
    /// Classification of a joystick position relative to its calibrated centre.
    /// </summary>
    internal enum JoystickMovement
    {
        /// <summary>No action is mapped to this position (dead-zone boundary cases).</summary>
        None,
        Rest,
        ForwardOrBackward,
        ForwardWithLeftTurn,
        ForwardWithRightTurn,
        BackwardWithLeftTurn,
        BackwardWithRightTurn,
        TurnLeftOrRight,
    }
}
