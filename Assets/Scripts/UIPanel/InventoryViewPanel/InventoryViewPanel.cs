using System;
using System.Collections.Generic;
using System.Linq;
using CodiceApp.EventTracking.Plastic;
using UnityEngine;
using CommonBase;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishCollection
{
    public class InventoryViewPanel : InventoryViewPanelBase, IPointerDownHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler
    {
        public GameObject fishTruckPrefab;
        public Dictionary<SpecialFishCaught, GameObject> viewFishInstances;

        private SpecialFishCaughtView draggingView; // 正在被拖动的鱼
        private Vector3 originalPosition; // 拖动开始时的位置
        private Vector3 mouseOffset; // 鼠标与鱼的相对位置
        private GameObject previewFish; // 预览的鱼

        private void Awake()
        {
            Viewport = transform.Find("Viewport").GetComponent<Image>();
            viewFishInstances = new Dictionary<SpecialFishCaught, GameObject>();
        }

        private void OnEnable()
        {
            this.EventRegister(GameEvent.BagChange, OnBagChange);
        }

        private void OnBagChange()
        {
            UpdateView(null);
        }

        public override void UpdateView(object o)
        {
            base.UpdateView(o);
            var fishToRemove = viewFishInstances.Keys.Except(BagSystem.Instance.fishDic.Values).ToList();

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
                    instance.GetComponent<SpecialFishCaughtView>().SetFish(fishKV.Value);
                    viewFishInstances.Add(fishKV.Value, instance);
                }

                instance.transform.localPosition = new Vector3(fishKV.Value.topLeftPos.x * 100,
                    (-fishKV.Value.topLeftPos.y) * 100, 0);
                instance.transform.localScale = fishKV.Value.inventorySize.To3();
                instance.transform.GetComponent<Image>().color = fishKV.Value.fishColor;
            }
        }

        #region 交互部分

        public void OnPointerDown(PointerEventData eventData)
        {
            var hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit != null && viewFishInstances.Values.Contains(hit))
            {
                foreach (var item in viewFishInstances)
                {
                    if (item.Value == hit)
                    {
                        draggingView = item.Value.GetComponent<SpecialFishCaughtView>();
                        originalPosition = hit.transform.localPosition;
                        mouseOffset = hit.transform.position - Input.mousePosition;
                        break;
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (draggingView != null)
            {
                draggingView.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
                draggingView.gameObject.SetActive(false); // 隐藏原鱼`
                previewFish = Instantiate(fishTruckPrefab, Viewport.transform);
                previewFish.transform.localScale = draggingView.fish.inventorySize.To3();
                previewFish.transform.GetComponent<Image>().color = draggingView.fish.fishColor;
                previewFish.GetComponent<CanvasGroup>().alpha = 0.5f; // 设置透明度
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (previewFish != null)
            {
                Vector3 newPosition = Input.mousePosition + mouseOffset;
                previewFish.transform.position = newPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggingView != null)
            {
                TryPlaceOrSwap(draggingView);
                if (previewFish != null)
                {
                    Destroy(previewFish);
                }

                draggingView.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
                draggingView.gameObject.SetActive(true); // 显示原鱼
                draggingView = null;
            }
        }

        private void TryPlaceOrSwap(SpecialFishCaughtView fishToMove)
        {
            // 获取目标位置
            Vector3 targetPosition = previewFish.transform.localPosition;
            Vector2 targetGridPosition = new Vector2(targetPosition.x / 100, -targetPosition.y / 100);

            // 检查目标位置是否为空闲
            if (BagSystem.Instance.CanFitFish(fishToMove.fish, targetGridPosition))
            {
                // 目标位置空闲，移动鱼
                BagSystem.Instance.MoveFish(fishToMove.fish, targetGridPosition);
            }
            else
            {
                // 目标位置不空闲，尝试交换
                SpecialFishCaught fishAtTarget = BagSystem.Instance.GetFishAtPosition(targetGridPosition);
                if (fishAtTarget != null && BagSystem.Instance.CanFitFish(fishToMove.fish, targetGridPosition))
                {
                    // 交换位置
                    BagSystem.Instance.SwapFish(fishToMove.fish, fishAtTarget);
                }
            }
        }

        #endregion
    }
}