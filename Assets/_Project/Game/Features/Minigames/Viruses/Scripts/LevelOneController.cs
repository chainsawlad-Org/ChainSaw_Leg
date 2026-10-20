using UnityEngine;
using System.Collections;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelOneController : MonoBehaviour
    {
        [SerializeField]
        private GameObject viruses;
        [SerializeField]
        private int maxViruses;

        private Coroutine coroutine;
        private int count;
        public BoxCollider2D area;
        public GameObject virusesList;
        //Временно
        public GameManager gameManager;

        public void StartLevel()
        {
            coroutine = StartCoroutine(SpawnViruses());
        }

        public void CompleteLevel()
        {
            StopCoroutine(coroutine);
            DeleteViruses();
            //Временно
            gameManager.NextLevel();

            Debug.Log("Level 1 is complete");
        }

        private Vector2 GetPosition()
        {
            Bounds bound = area.bounds;
            float x = Random.Range(bound.min.x, bound.max.x);
            float y = Random.Range(bound.min.y, bound.max.y);

            return new Vector2(x, y);
        }

        private int CheckVirusesList()
        {
            int n = virusesList.transform.childCount;
            return n;
        }

        private void DeleteViruses()
        {
            int n = CheckVirusesList();

            for (int i = 0; i < n; i++)
            {
                Destroy(virusesList.transform.GetChild(i).gameObject);
            }
        }

        IEnumerator SpawnViruses()
        {
            while (true)
            {
                if (CheckVirusesList() != maxViruses)
                {
                    Instantiate(viruses, GetPosition(), Quaternion.identity, virusesList.transform);
                    count++;
                }
                else
                {
                    CompleteLevel();
                }
                    yield return new WaitForSeconds(1f);
            }
        }
    }
}
