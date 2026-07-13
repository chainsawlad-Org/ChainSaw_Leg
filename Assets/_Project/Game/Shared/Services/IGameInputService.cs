using UnityEngine;

public interface IGameInputService
{
    bool IsReady { get; }
    bool IsGameplayInputEnabled { get; }
    Vector2 MoveInput { get; }
    bool DashPressed { get; }
    bool InteractPressed { get; }
    bool SubmitPressed { get; }
    bool UiSubmitPressed { get; }
    bool PreviousPressed { get; }
    bool NextPressed { get; }

    void ConsumeDash();
    void ConsumeInteract();
    void ConsumeSubmit();
    void ConsumePrevious();
    void ConsumeNext();
    void ResetTransientInput();
}
