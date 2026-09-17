using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChainSawLeg.Features.Exploration
{
    public class FloorManager : MonoBehaviour
    {
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private RoomManager[] rooms;


        public void SetFloorState(bool state, float duration, bool changeOrder = true)
        {
            foreach (var room in rooms) room.ChangeLevel(state, duration);
            if (changeOrder)
            {
                if (state) sortingGroup.sortingOrder++;
                else sortingGroup.sortingOrder--;
            }
        }
    }
}
