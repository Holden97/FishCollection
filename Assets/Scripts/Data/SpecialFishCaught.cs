using System;
using UnityEngine;

namespace FishCollection
{
    [Serializable]
    public class SpecialFishCaught
    {
        public int fishId;
        public Vector2Int inventorySize;
        public Vector2Int topLeftPos;
        public Color fishColor;
        
        public SpecialFishCaught(SpecialFish fish,Vector2Int topLeftPos)
        {
            this.fishId = fish.specialFishId;
            this.inventorySize = fish.InventoryGridSize;
            this.topLeftPos = topLeftPos;
            this.fishColor = fish.fishBaseColor;
        }
    }
}