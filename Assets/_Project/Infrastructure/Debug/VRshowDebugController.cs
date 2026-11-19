using System;
using UnityEngine;
using UnityDebugSheet;
using UnityEngine.Audio;


namespace ChainSawLeg.Features.Minigames
{
    public class VRshowDebugController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Transform player;
        [SerializeField] private Transform mainPoint;
        
        private void Start()
        {
            var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
            
            rootPage.ClearItems();
        
            rootPage.AddPageLinkButton<AudioPage>("Audio", onLoad: x => x.page.Initialize(audioMixer));
            rootPage.AddButton("Teleport to main", clicked: () => player.transform.position = mainPoint.position);
        }
    }
}
