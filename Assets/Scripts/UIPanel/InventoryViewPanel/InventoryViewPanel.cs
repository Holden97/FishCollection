using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
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
        public SerializedDictionary<SpecialFishCaught, GameObject> fishViews;

        private SpecialFishCaughtView draggingView; // 正在被拖动的鱼
        private Vector3 originalPosition; // 拖动开始时的位置
        private Vector3 mouseOffset; // 鼠标与鱼的相对位置
        private GameObject previewFish; // 预览的鱼
        private GameObject previewFishAtTarget; // 目标位置预览鱼

        private void OnEnable()
        {
            this.EventRegister(GameEvent.BagChange, OnBagChange);
        }

        private void OnBagChange()
        {
            UpdateView(null);
        }

        public override void Initialize()
        {
            base.Initialize();
            fishViews = new SerializedDictionary<SpecialFishCaught, GameObject>();
        }

        public override void UpdateView(object o)
        {
            base.UpdateView(o);

            var fishToRemove = fishViews.Keys.Except(BagSystem.Instance.fishDic.Values).ToList();

            foreach (var fish in fishToRemove)
            {
                Destroy(fishViews[fish]);
                fishViews.Remove(fish);
            }

            foreach (var fishKV in BagSystem.Instance.fishDic)
            {
                GameObject instance = default;
                if (fishViews.ContainsKey(fishKV.Value))
                {
                    instance = fishViews[fishKV.Value];
                }
                else
                {
                    instance = Instantiate(fishTruckPrefab, Viewport.transform);
                    instance.GetComponent<SpecialFishCaughtView>().SetFish(fishKV.Value);
                    fishViews.Add(fishKV.Value, instance);
                }

                instance.transform.localPosition = new Vector3(fishKV.Value.topLeftPos.x * 100,
                    (-fishKV.Value.topLeftPos.y) * 100, 0);
                instance.transform.localScale = fishKV.Value.inventorySize.To3();
                instance.transform.GetComponent<Image>().color = fishKV.Value.fishColor;
            }

            Capcity_1.text = BagSystem.Instance.GetCapcity().ToString();
            Occupy_2.text = BagSystem.Instance.GetOccupancy().ToString();
        }

        #region 交互部分

        public void OnPointerDown(PointerEventData eventData)
        {
            var hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit != null && fishViews.Values.Contains(hit))
            {
                foreach (var item in fishViews)
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
                draggingView.gameObject.SetActive(false); // 隐藏原鱼
                previewFish = Instantiate(fishTruckPrefab, transform);
                previewFish.transform.localScale = draggingView.fish.inventorySize.To3();
                previewFish.transform.GetComponent<Image>().color = draggingView.fish.fishColor;
                previewFish.GetComponent<CanvasGroup>().alpha = 0.5f; // 设置透明度
                previewFishAtTarget = null; // 初始化目标位置预览鱼
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (previewFish != null)
            {
                Vector3 fishPosition = Input.mousePosition + mouseOffset;
                var currentGridPosition = GetScreen2CurrentGridPosition(fishPosition);
                bool gridPosIsValid = BagSystem.Instance.IsValidGridPosition(currentGridPosition);
                previewFishAtTarget?.gameObject.SetActive(gridPosIsValid);
                previewFish.transform.position = fishPosition;
                if (!gridPosIsValid)
                {
                    return;
                }

                SpecialFishCaught fishAtTarget = BagSystem.Instance.GetFishAtPosition(currentGridPosition);
                List<int> excludedFishList = new List<int>();
                if (fishAtTarget != null)
                {
                    excludedFishList.Add(fishAtTarget.fishId);
                }

                // 检查当前位置是否可以放置鱼
                if (BagSystem.Instance.CanFitFish(draggingView.fish, currentGridPosition, excludedFishList))
                {
                    // 可以放置，显示目标位置预览鱼
                    if (previewFishAtTarget == null)
                    {
                        previewFishAtTarget = Instantiate(fishTruckPrefab, Viewport.transform);
                        previewFishAtTarget.transform.localScale = draggingView.fish.inventorySize.To3();
                        previewFishAtTarget.transform.GetComponent<Image>().color = draggingView.fish.fishColor;
                        previewFishAtTarget.GetComponent<CanvasGroup>().alpha = 0.5f; // 设置透明度
                    }

                    previewFishAtTarget.transform.localPosition = new Vector3((int)currentGridPosition.x * 100,
                        ((int)-currentGridPosition.y) * 100, 0);
                }
                else
                {
                    // 不能放置，隐藏目标位置预览鱼
                    if (previewFishAtTarget != null)
                    {
                        Destroy(previewFishAtTarget);
                        previewFishAtTarget = null;
                    }
                }
            }
        }

        private Vector2 GetScreen2CurrentGridPosition(Vector3 newPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Viewport.GetComponent<RectTransform>(),
                newPosition,
                null,
                out var localPoint);
            Vector2 currentGridPosition = new Vector2Int((int)(localPoint.x / 100), (int)(-localPoint.y / 100));
            return currentGridPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggingView != null)
            {
                // 检查拖拽结束时的位置是否有效
                Vector3 endPosition = Input.mousePosition + mouseOffset;
                Vector2 endGridPosition = GetScreen2CurrentGridPosition(endPosition);

                if (!BagSystem.Instance.IsValidGridPosition(endGridPosition))
                {
                    // 如果结束位置无效，则触发丢弃逻辑
                    BagSystem.Instance.RemoveFish(draggingView.fish);
                }
                else
                {
                    TryPlaceOrSwap(draggingView);
                }

                if (previewFish != null)
                {
                    Destroy(previewFish);
                }

                if (previewFishAtTarget != null)
                {
                    Destroy(previewFishAtTarget);
                }

                draggingView.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
                draggingView.gameObject.SetActive(true); // 显示原鱼


                draggingView = null;
            }
        }

        private void TryPlaceOrSwap(SpecialFishCaughtView fishToMove)
        {
            // 获取目标位置
            Vector3 targetPosition = Viewport.transform.InverseTransformPoint(previewFish.transform.position);
            Vector2Int targetGridPosition =
                new Vector2Int((int)(targetPosition.x / 100), (int)(-targetPosition.y / 100));

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

                if (fishAtTarget != null && BagSystem.Instance.CanFitFish(fishToMove.fish, targetGridPosition,
                                             new List<int>() { fishAtTarget.fishId })
                                         && BagSystem.Instance.CanFitFish(fishAtTarget, fishToMove.fish.topLeftPos,
                                             new List<int>() { fishToMove.fish.fishId }))
                {
                    // 交换位置
                    BagSystem.Instance.SwapFish(fishToMove.fish, fishAtTarget, targetGridPosition);
                }
            }
        }

        #endregion

        #region imgui调试

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            for (int r = 0; r < BagSystem.Instance.bagOccupancy.GetLength(1); r++)
            {
                GUILayout.BeginHorizontal();
                for (int c = 0; c < BagSystem.Instance.bagOccupancy.GetLength(0); c++)
                {
                    GUILayout.Label(BagSystem.Instance.bagOccupancy[c, r].ToString(), GUILayout.Width(40));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}