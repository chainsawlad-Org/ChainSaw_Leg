using System.Collections;
using System.Collections.Generic;
using ChainSawLeg.Features.Exploration;
using UnityDebugSheet;
using UnityEngine;
using UnityEngine.Audio;

public sealed class DebugSheetController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerKick playerKick;
    [SerializeField] private AudioMixer audioMixer;
    
    private void Start()
    {
        StartCoroutine(WaitToInit());
    }

    IEnumerator WaitToInit()
    {
        while (DebugSheet.Instance == null) yield return new WaitForEndOfFrame();
        
        var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
        
        rootPage.AddPageLinkButton<PlayerPage>("Player", onLoad: x => x.page.Initialize(player, playerKick));
        rootPage.AddPageLinkButton<AudioPage>("Audio", onLoad: x => x.page.Initialize(audioMixer));

    }
}