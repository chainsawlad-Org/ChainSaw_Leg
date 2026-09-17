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
            secondFloorManager.SetFloorState(true, 0f, false);
            yield return new WaitForEndOfFrame();
            secondFloorManager.SetFloorState(false, 0f);
        }

        public void ChangeFloor(int index)
        {
            if (index == 0)
            {
                secondFloorManager.SetFloorState(false, 1f);
                DOVirtual.DelayedCall(2f, () => firstFloorManager.SetFloorState(true, 1f));
            }
            if (index == 1)
            {
                firstFloorManager.SetFloorState(false, 1f);
                DOVirtual.DelayedCall(2f, () => secondFloorManager.SetFloorState(true, 1f));
            }
        }
    }
}
