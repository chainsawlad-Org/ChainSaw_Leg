using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChainSawLeg.Features.Exploration
{
    public class FloorManager : MonoBehaviour
    {
        [SerializeField] private RoomManager[] rooms;


        public void SetFloorState(bool state, float duration)
        {
            foreach (var room in rooms) room.ChangeLevel(state, duration);
        }
    }
}
