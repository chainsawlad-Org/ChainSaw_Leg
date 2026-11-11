using System;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Interactable NPC that offers to distract the robot with holograms. Raises
    /// <see cref="Interacted"/>; <see cref="RobotKiller"/> subscribes and enables its
    /// holograms — the dudes do not know about the robot.
    /// </summary>
    public class HelpingDudes : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject pingObject;

        /// <summary>Raised when the player interacts (asks the dudes to distract the robot).</summary>
        public event Action Interacted;

        private bool canInteract;

        /// <summary>Interaction prompt; the dudes show no text prompt (empty by design).</summary>
        public string GetInteractionPrompt()
        {
            return "";
        }

        /// <summary>True while the dudes are waiting for the player's help.</summary>
        public bool CanInteract()
        {
            return canInteract;
        }

        /// <summary>Called on the robot's first hammer hit; makes the dudes ask for help.</summary>
        public void Ping()
        {
            pingObject.SetActive(true);
            canInteract = true;
        }

        /// <summary>Player interacted: stop offering help and request the robot's distraction.</summary>
        public void Interact()
        {
            pingObject.SetActive(false);
            canInteract = false;
            Interacted?.Invoke();
        }
    }
}
