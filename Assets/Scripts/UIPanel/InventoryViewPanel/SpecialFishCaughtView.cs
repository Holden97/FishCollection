using System;
using UnityEngine;

namespace FishCollection
{
    public class SpecialFishCaughtView : MonoBehaviour
    {
        public SpecialFishCaught fish;

        public void SetFish(SpecialFishCaught fish)
        {
            this.fish = fish;
        }
    }
}