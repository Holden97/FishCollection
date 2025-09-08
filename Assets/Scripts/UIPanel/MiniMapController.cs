using System;
using UnityEngine;

namespace FishCollection
{
    public class MiniMapController : MonoBehaviour
    {
        private void Update()
        {
            if (GameManager.Instance.Shark != null)
            {
                this.transform.position = GameManager.Instance.Shark.transform.position + Vector3.up * 10;
            }
        }
    }
}