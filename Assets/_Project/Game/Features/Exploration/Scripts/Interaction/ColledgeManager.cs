using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Exploration
{
    public class ColledgeManager : MonoBehaviour
    {
        [SerializeField] private FloorManager firstFloorManager;
        [SerializeField] private FloorManager secondFloorManager;

        private void Start()
        {
            StartCoroutine(SetupSecondFloor());
        }

        IEnumerator SetupSecondFloor()
        {
            secondFloorManager.SetFloorState(true, 0f);
            yield return new WaitForEndOfFrame();
            secondFloorManager.SetFloorState(false, 0f);
            secondFloorManager.gameObject.SetActive(false);
        }

        public void ChangeFloor(int index)
        {
            if (index == 0)
            {
                secondFloorManager.SetFloorState(false, 1f);
                DOVirtual.DelayedCall(2f, () =>
                {
                    secondFloorManager.gameObject.SetActive(false);
                    firstFloorManager.gameObject.SetActive(true);
                    firstFloorManager.SetFloorState(true, 1f);
                });
            }
            if (index == 1)
            {
                firstFloorManager.SetFloorState(false, 1f);
                DOVirtual.DelayedCall(2f, () =>
                {
                    secondFloorManager.gameObject.SetActive(true);
                    firstFloorManager.gameObject.SetActive(false);
                    secondFloorManager.SetFloorState(true, 1f);
                });
            }
        }
    }
}
