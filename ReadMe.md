# `JoystickHandler` Class

The `JoystickHandler` class provides functionality to handle joystick input using the SharpDX.DirectInput library. It enables polling joystick states, reacting to axis movements, and triggering actions for button presses.

## Namespace
`Mtf.Joystick`

## Methods

### `InitializeJoystick`

Initializes and starts listening to joystick input. It polls the joystick for its current state, interprets axis movements, and triggers associated actions.

#### Parameters

- `Func<int> getDeviceIndex`: Function to provide the correct joystick index.
- `Func<bool> continuePulling`: Function to determine if polling should continue.
- `Func<int> getDeltaModifier`: Function to provide a value for scaling joystick axis movements.
- `Func<int> getMinimumDelta`: Function to provide the minimum threshold for movement detection.
- `Action<int, int> restAction`: Action to invoke when the joystick is at rest.
- `Action<int, int> forwardOrBackwardAction`: Action to invoke for forward/backward movement.
- `Action<int, int> forwardWithLeftTurnAction`: Action to invoke for forward with a left turn.
- `Action<int, int> forwardWithRightTurnAction`: Action to invoke for forward with a right turn.
- `Action<int, int> backwardWithLeftTurnAction`: Action to invoke for backward with a left turn.
- `Action<int, int> backwardWithRightTurnAction`: Action to invoke for backward with a right turn.
- `Action<int, int> turnLeftOrRightAction`: Action to invoke for turning left or right.
- `Action afterPullingAction`: Action to invoke after polling stops.
- `Action[] buttonActions`: Array of actions to invoke for button presses.

#### Returns

- `bool`: To indicate if the operation was successful or not.

### `CalibrateJoystick`

Calibrate the joystick.

#### Returns

- `bool`: To indicate if the operation was successful or not.

### `StopJoystick`

Stops joystick polling.

---

## Example Usage

Here is a simplified example demonstrating how to use the `JoystickHandler`:

```csharp
using System;
using System.Threading;

var succeeded = JoystickHandler.InitializeJoystick(
    getDeviceIndex: () => 0,
    continuePulling: () => true,
    getDeltaModifier: () => 10,
    getMinimumDelta: () => 5,
    restAction: (deltaX, deltaY) => Console.WriteLine("Joystick at rest."),
    forwardOrBackwardAction: (deltaX, deltaY) => Console.WriteLine($"Moving forward/backward: {deltaY}"),
    forwardWithLeftTurnAction: (deltaX, deltaY) => Console.WriteLine($"Forward with left turn: {deltaX}, {deltaY}"),
    forwardWithRightTurnAction: (deltaX, deltaY) => Console.WriteLine($"Forward with right turn: {deltaX}, {deltaY}"),
    backwardWithLeftTurnAction: (deltaX, deltaY) => Console.WriteLine($"Backward with left turn: {deltaX}, {deltaY}"),
    backwardWithRightTurnAction: (deltaX, deltaY) => Console.WriteLine($"Backward with right turn: {deltaX}, {deltaY}"),
    turnLeftOrRightAction: (deltaX, deltaY) => Console.WriteLine($"Turning left/right: {deltaX}"),
    afterPullingAction: () => Console.WriteLine("Polling stopped."),
    buttonActions: new Action[]
    {
        () => Console.WriteLine("Button 1 pressed."),
        () => Console.WriteLine("Button 2 pressed."),
        () => Console.WriteLine("Button 3 pressed."),
    }
);

// Simulate stopping the joystick handler after some time
Thread.Sleep(5000);
JoystickHandler.StopJoystick();
```

---

## Notes

1. The library uses `SharpDX.DirectInput` for device communication. Ensure the `SharpDX` library is installed.
2. The joystick must be connected, and the `DeviceClass.GameControl` class is used to detect input devices.
3. All movements and button presses are translated into corresponding actions, making this class versatile for various input scenarios.
4. Remember to dispose of the cancellation token properly after stopping the joystick.