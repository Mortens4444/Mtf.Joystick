using Xunit;

// JoystickHandler is a static class with shared state, so its tests must not run concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
