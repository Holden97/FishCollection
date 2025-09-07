using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommonBase;
using UnityEngine.UI;

namespace FishCollection
{
    public class InventoryViewPanel : InventoryViewPanelBase
    {
        public GameObject fishTruckPrefab;
        public Dictionary<SpecialFishCaught, GameObject> viewFishInstances;

        private void Awake()
        {
            Viewport = transform.Find("Viewport").GetComponent<Image>();
            viewFishInstances = new Dictionary<SpecialFishCaught, GameObject>();
        }

        public override void UpdateView(object o)
        {
            base.UpdateView(o);
            // 使用 LINQ 的 Except 方法找到需要移除的鱼
            var fishToRemove = viewFishInstances.Keys.Except(BagSystem.Instance.fishDic.Values).ToList();

            // 移除这些鱼
            foreach (var fish in fishToRemove)
            {
                Destroy(viewFishInstances[fish]);
                viewFishInstances.Remove(fish);
            }

            foreach (var fishKV in BagSystem.Instance.fishDic)
            {
                GameObject instance = default;
                if (viewFishInstances.ContainsKey(fishKV.Value))
                {
                    instance = viewFishInstances[fishKV.Value];
                }
                else
                {
                    instance = Instantiate(fishTruckPrefab, Viewport.transform);
                    viewFishInstances.Add(fishKV.Value, instance);
                }

                instance.transform.localPosition = new Vector3(fishKV.Value.topLeftPos.x * 100,
                    (-fishKV.Value.topLeftPos.y) * 100, 0);
                instance.transform.localScale = fishKV.Value.inventorySize.To3();
                instance.transform.GetComponent<Image>().color = fishKV.Value.fishColor;
            }
        }
    }
}