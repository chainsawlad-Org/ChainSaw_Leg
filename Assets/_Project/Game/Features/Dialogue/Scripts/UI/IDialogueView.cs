using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Features.Dialogue
{
    public interface IDialogueView
    {
        UniTask HideAnimatedAsync();
    }
}