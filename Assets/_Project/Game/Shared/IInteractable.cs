// Placement: Docs/Ru/02_ProjectStructure.md:240-255. Quote: "├── Shared".

public interface IInteractable
{
    string GetInteractionPrompt();
    bool CanInteract();
    void Interact();
}
