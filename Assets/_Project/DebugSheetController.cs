using UnityDebugSheet;
using UnityEngine;

public sealed class DebugSheetController : MonoBehaviour
{
    [SerializeField] private Transform player;
    
    private void Start()
    {
        // Get or create the root page.
        var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();

        // Add a link transition to the ExampleDebugPage.
        rootPage.AddPageLinkButton<BaseDebugPage>("Debug menu", onLoad: x => x.page.Initialize(player));
    }
}