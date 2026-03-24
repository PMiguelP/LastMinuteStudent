using System;

[Flags]
public enum RunnerLane
{
    None = 0,
    Left = 1,
    Middle = 2,
    Right = 4,
    All = Left | Middle | Right
}