using System;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class TriggerHalfEvilMisty : MonoBehaviour
    {
        [SerializeField] private HalfEvilMisty evilMisty;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            evilMisty.StartAttack();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
