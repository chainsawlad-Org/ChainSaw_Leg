using System.Collections;
using System.Collections.Generic;
using ChainSawLeg.Features.Exploration;
using UnityDebugSheet;
using UnityEngine;

public sealed class DebugSheetController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerKick playerKick;
    
    private void Start()
    {
        StartCoroutine(WaitToInit());
    }

    IEnumerator WaitToInit()
    {
        while (DebugSheet.Instance == null) yield return new WaitForEndOfFrame();
        
        var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
        
        rootPage.AddPageLinkButton<BaseDebugPage>("Debug menu", onLoad: x => x.page.Initialize(player, playerKick));

    }
}