using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class HelpingDudes : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject pingObject;
        [SerializeField] private RobotKiller robotKiller;
        private bool canInteract;
        
        public string GetInteractionPrompt()
        {
            return "";
        }

        public bool CanInteract()
        {
            return canInteract;
        }

        public void Ping()
        {
            pingObject.SetActive(true);
            canInteract = true;
        }

        public void Interact()
        {
            pingObject.SetActive(false);
            robotKiller.EnableHolograms();
            canInteract = false;
        }
    }
}
