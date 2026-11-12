using System;
using UnityEngine;

namespace ChainSawLeg.Game.Shared
{
    public class SinMove : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private Vector2 direction;

        private void Update()
        {
            transform.Translate(direction * speed * Mathf.Sin(Time.time) * Time.deltaTime);
        }
    }
}
