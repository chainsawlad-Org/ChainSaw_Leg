using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet;
using UnityEngine;

public sealed class DebugSheetController : MonoBehaviour
{
    [SerializeField] private Transform player;
    
    private void Start()
    {
        StartCoroutine(WaitToInit());
    }

    IEnumerator WaitToInit()
    {
        while (DebugSheet.Instance == null) yield return new WaitForEndOfFrame();
        
        var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
        
        rootPage.AddPageLinkButton<BaseDebugPage>("Debug menu", onLoad: x => x.page.Initialize(player));

    }
}