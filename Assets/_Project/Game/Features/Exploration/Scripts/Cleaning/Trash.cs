using ChainSawLeg.Features.Minigames;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Exploration
{
    public class Trash : MonoBehaviour
    {
        [SerializeField] private Collider2D[] targets;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Garbage>())
            {
                other.GetComponent<Rigidbody2D>().linearDamping = 10f;
                other.transform.DOScale(0f, 1f).OnComplete(() => Destroy(other.gameObject, 0.1f)).SetAutoKill(true);
            }
        }
    }
}
