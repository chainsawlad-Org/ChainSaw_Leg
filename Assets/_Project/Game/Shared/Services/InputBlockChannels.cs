using System;

[Flags]
public enum InputBlockChannels
{
    None = 0,
    Move = 1 << 0,
    Dash = 1 << 1,
    Interact = 1 << 2,
    Submit = 1 << 3,
    Kick = 1 << 4,
    Exploration = Move | Dash | Interact | Kick,
    Gameplay = Move | Dash | Interact | Submit | Kick
}
