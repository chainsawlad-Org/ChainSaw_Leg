using System;
using UnityEngine;
using UnityDebugSheet;
using UnityEngine.Audio;


namespace ChainSawLeg.Features.Minigames
{
    public class VRshowDebugController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        
        private void Start()
        {
            var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
        
            rootPage.AddPageLinkButton<AudioPage>("Audio", onLoad: x => x.page.Initialize(audioMixer));
        }
    }
}
