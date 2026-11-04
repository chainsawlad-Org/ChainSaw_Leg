using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class CameraFixPhaseSetup : MonoBehaviour
    {
        [Header("Victory Condition")]
        [SerializeField] private int targetVoltage = 3;

        [Header("Inventory Root")]
        [SerializeField] private GameObject inventoryRoot;

        [Header("Circuit Blocks")]
        [SerializeField] private List<CircuitBlock> navigationOrder = new();

        [Header("Available Block Prefabs")]
        [SerializeField] private List<CircuitBlock> availableBlockPrefabs = new();

        [Header("Inventory Display Blocks")]
        [SerializeField] private List<CircuitBlock> inventoryDisplayBlocks = new();

        public int TargetVoltage => targetVoltage;

        public GameObject InventoryRoot => inventoryRoot;

        public IReadOnlyList<CircuitBlock> NavigationOrder => navigationOrder;

        public IReadOnlyList<CircuitBlock> AvailableBlockPrefabs =>
            availableBlockPrefabs;

        public IReadOnlyList<CircuitBlock> InventoryDisplayBlocks =>
            inventoryDisplayBlocks;

        public void SetPhaseActive(bool isActive)
        {
            if (gameObject.activeSelf != isActive)
                gameObject.SetActive(isActive);

            if (inventoryRoot != null &&
                inventoryRoot.activeSelf != isActive)
            {
                inventoryRoot.SetActive(isActive);
            }
        }
    }
}