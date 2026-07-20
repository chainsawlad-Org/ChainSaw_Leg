using System;

[Flags]
public enum InputBlockChannels
{
    None = 0,
    Move = 1 << 0,
    Dash = 1 << 1,
    Interact = 1 << 2,
    Submit = 1 << 3,
    Gameplay = Move | Dash | Interact | Submit
}
