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

        [Header("FlyEvilMisty Arena")]
        [Tooltip("Dedicated flying player for the FlyEvilMisty boss fight. Enabled in place of the main player.")]
        [SerializeField] private GameObject flyEvilMistyPlayer;
        [Tooltip("Where the FlyEvilMisty player starts when teleporting into the arena.")]
        [SerializeField] private Transform flyEvilMistySpawnPoint;
        [Tooltip("Optional FlyEvilMisty arena root that is activated together with the fight.")]
        [SerializeField] private GameObject flyEvilMistyArenaRoot;
        [Tooltip("Main platformer player swapped out during the fight. Falls back to the 'player' transform.")]
        [SerializeField] private GameObject mainPlayer;

        private Vector3 mainPlayerRestorePosition;
        private bool flyArenaActive;

        private void Start()
        {
            var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();

            rootPage.ClearItems();

            rootPage.AddPageLinkButton<AudioPage>("Audio", onLoad: x => x.page.Initialize(audioMixer));
            rootPage.AddButton("Teleport to main", clicked: TeleportToMain);
            rootPage.AddButton("Teleport to FlyEvilMisty", clicked: TeleportToFlyEvilMisty);
            rootPage.AddButton("Return from FlyEvilMisty", clicked: ReturnFromFlyEvilMisty);
        }

        private void TeleportToMain()
        {
            if (player == null || mainPoint == null)
            {
                Debug.LogWarning("VRshowDebugController: player or mainPoint is not assigned.");
                return;
            }

            player.transform.position = mainPoint.position;
        }

        private GameObject ResolveMainPlayer()
        {
            if (mainPlayer != null) return mainPlayer;
            if (player != null) return player.gameObject;

            return null;
        }

        private void TeleportToFlyEvilMisty()
        {
            if (flyEvilMistyPlayer == null)
            {
                Debug.LogWarning("VRshowDebugController: flyEvilMistyPlayer is not assigned.", this);
                return;
            }

            if (flyArenaActive) return;

            GameObject mainPlayerObject = ResolveMainPlayer();

            if (mainPlayerObject != null)
            {
                mainPlayerRestorePosition = mainPlayerObject.transform.position;
                mainPlayerObject.SetActive(false);
            }

            if (flyEvilMistySpawnPoint != null)
                flyEvilMistyPlayer.transform.position = flyEvilMistySpawnPoint.position;

            flyEvilMistyPlayer.SetActive(true);

            if (flyEvilMistyArenaRoot != null) flyEvilMistyArenaRoot.SetActive(true);

            flyArenaActive = true;
        }

        private void ReturnFromFlyEvilMisty()
        {
            if (!flyArenaActive) return;

            if (flyEvilMistyPlayer != null) flyEvilMistyPlayer.SetActive(false);

            GameObject mainPlayerObject = ResolveMainPlayer();

            if (mainPlayerObject != null)
            {
                mainPlayerObject.transform.position = mainPlayerRestorePosition;
                mainPlayerObject.SetActive(true);
            }

            flyArenaActive = false;
        }
    }
}
