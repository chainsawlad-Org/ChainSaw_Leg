using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class RotationSquare : MonoBehaviour
    {
        [SerializeField] private GameObject square;
        [SerializeField] private int squareCount;
        public float speed;
        public float radius;

        [SerializeField] private float proximitySpeed = 2f;

        private List<GameObject> squares = new List<GameObject>();

        private void Start()
        {
            float angleStep = 360f / squareCount;

            for (int i = 0; i < squareCount; i++)
            {
                
                float currentAngle = i * angleStep * Mathf.Deg2Rad;

                float x = Mathf.Cos(currentAngle) * radius;
                float y = Mathf.Sin(currentAngle) * radius;

                Vector3 spawnPosition = transform.position + new Vector3(x, y, 0);

                GameObject newSquare = Instantiate(square, spawnPosition, Quaternion.identity, transform);
                newSquare.GetComponent<SquareMove>().vector = new Vector2 (x, y).normalized;
                newSquare.transform.up = (transform.position - spawnPosition).normalized;
                squares.Add(newSquare);
            }
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
            Proximity();
        }

        public void Proximity()
        {
            foreach (var obj in squares)
            {
                obj.GetComponent<Rigidbody2D>().linearVelocity = -1 * obj.GetComponent<SquareMove>().vector;
            }
        }
    }
}
