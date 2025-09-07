using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using CommonBase;
using UnityEngine;

namespace FishCollection
{
    public class BagSystem : MonoSingleton<BagSystem>
    {
        public Vector2Int bagSize;

        [SerializedDictionary("fishId", "Info")]
        public SerializedDictionary<int, SpecialFishCaught> fishDic;

        private int[,] bagOccupancy;

        private void Start()
        {
            bagSize = new Vector2Int(7, 6);
            fishDic = new SerializedDictionary<int, SpecialFishCaught>();
            bagOccupancy = new int[bagSize.x, bagSize.y];
            for (int x = 0; x < bagSize.x; x++)
            for (int y = 0; y < bagSize.y; y++)
                bagOccupancy[x, y] = -1;

            //注册事件
            GameManager.Instance.RegisterOnFishEaten(AddFish);
        }

        private void OnDestroy()
        {
            GameManager.Instance.UnregisterOnFishEaten(AddFish);
        }

        public void AddFish(SpecialFish fish)
        {
            if (fish == null || fish.InventoryGridSize.x <= 0 || fish.InventoryGridSize.y <= 0)
            {
                return; // 鱼无效或不需要空间
            }

            // 找到足够的连续空闲格子
            for (int x = 0; x < bagSize.x; x++)
            {
                for (int y = 0; y < bagSize.y; y++)
                {
                    if (CanPlaceFishAt(fish, x, y))
                    {
                        // 标记这些格子为已占用
                        MarkOccupied(fish, x, y);

                        // 将鱼添加到列表中
                        fishDic.Add(fish.specialFishId, new SpecialFishCaught(fish, new Vector2Int(x, y)));
                        return;
                    }
                }
            }
        }

        public bool RemoveFish(SpecialFishCaught fish)
        {
            if (fish == null || !fishDic.ContainsKey(fish.fishId))
            {
                return false; // 鱼无效
            }

            ClearOccupiedArea(fishDic[fish.fishId].topLeftPos, fishDic[fish.fishId].inventorySize);
            // 从列表中移除鱼
            fishDic.Remove(fish.fishId);

            return true;
        }

        private void ClearOccupiedArea(Vector2Int startPos, Vector2Int gridSize)
        {
            for (int x = startPos.x; x < startPos.x + gridSize.x; x++)
            {
                for (int y = startPos.y; y < startPos.y + gridSize.y; y++)
                {
                    bagOccupancy[x, y] = -1; // 清空格子
                }
            }
        }

        private bool CanPlaceFishAt(SpecialFish fish, int startX, int startY)
        {
            Vector2Int gridSize = fish.InventoryGridSize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    if (x >= bagSize.x || y >= bagSize.y || bagOccupancy[x, y] != -1)
                    {
                        return false; // 超出边界或已被占用
                    }
                }
            }

            return true; // 可以放置
        }

        private bool CanPlaceFishAt(SpecialFishCaught fish, int startX, int startY)
        {
            Vector2Int gridSize = fish.inventorySize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    if (x >= bagSize.x || y >= bagSize.y ||
                        (bagOccupancy[x, y] != -1 && bagOccupancy[x, y] != fish.fishId))
                    {
                        Debug.LogWarning("已占用");
                        return false; // 超出边界或已被占用
                    }
                }
            }

            return true; // 可以放置
        }

        private void MarkOccupied(SpecialFish fish, int startX, int startY)
        {
            int index = fish.specialFishId;
            Vector2Int gridSize = fish.InventoryGridSize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    bagOccupancy[x, y] = index;
                }
            }
        }

        private void MarkOccupied(SpecialFishCaught fish, int startX, int startY)
        {
            int index = fish.fishId;
            Vector2Int gridSize = fish.inventorySize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    bagOccupancy[x, y] = index;
                }
            }
        }

        public void ResizeBag(Vector2Int newSize)
        {
            // 创建一个新的更大的二维数组
            int[,] newBagOccupancy = new int[newSize.x, newSize.y];

            // 将旧数组中的数据复制到新数组中
            for (int x = 0; x < Mathf.Min(bagSize.x, newSize.x); x++)
            {
                for (int y = 0; y < Mathf.Min(bagSize.y, newSize.y); y++)
                {
                    newBagOccupancy[x, y] = bagOccupancy[x, y];
                }
            }

            // 初始化新添加的格子
            for (int x = 0; x < newSize.x; x++)
            {
                for (int y = 0; y < newSize.y; y++)
                {
                    if (x >= bagSize.x || y >= bagSize.y)
                    {
                        newBagOccupancy[x, y] = -1; // 初始所有新格子都为空
                    }
                }
            }

            // 更新背包大小和占用情况
            bagSize = newSize;
            bagOccupancy = newBagOccupancy;
        }

        // 检查目标位置是否可以放置鱼
        public bool CanFitFish(SpecialFishCaught fish, Vector2 targetGridPosition)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;
            return CanPlaceFishAt(fish, startX, startY);
        }

        // 移动鱼到目标位置
        public void MoveFish(SpecialFishCaught fishToMove, Vector2 targetGridPosition)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;

            if (CanFitFish(fishToMove, targetGridPosition))
            {
                ClearOccupiedArea(fishToMove.topLeftPos, fishToMove.inventorySize);
                MarkOccupied(fishToMove, startX, startY);
                fishToMove.topLeftPos = new Vector2Int(startX, startY);
                this.EventTrigger(GameEvent.BagChange);
            }
        }

        // 交换两条鱼的位置
        public void SwapFish(SpecialFishCaught fishToMove, SpecialFishCaught fishAtTarget)
        {
            if (CanFitFish(fishToMove, fishAtTarget.topLeftPos) && CanFitFish(fishAtTarget, fishToMove.topLeftPos))
            {
                Vector2Int tempPosition = fishToMove.topLeftPos;
                MoveFish(fishToMove, fishAtTarget.topLeftPos);
                MoveFish(fishAtTarget, tempPosition);
                this.EventTrigger(GameEvent.BagChange);
            }
        }

        // 获取目标位置的鱼
        public SpecialFishCaught GetFishAtPosition(Vector2 targetGridPosition)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;

            if (!IsValidGridPosition(targetGridPosition))
            {
                return null;
            }

            for (int x = startX; x < startX + 1; x++)
            {
                for (int y = startY; y < startY + 1; y++)
                {
                    if (bagOccupancy[x, y] != -1)
                    {
                        int fishId = bagOccupancy[x, y];
                        if (fishDic.TryGetValue(fishId, out SpecialFishCaught fish))
                        {
                            return fish;
                        }
                    }
                }
            }

            return null;
        }

        public bool IsValidGridPosition(Vector2 endGridPosition)
        {
            int x = (int)endGridPosition.x;
            int y = (int)endGridPosition.y;

            // 检查网格位置是否在背包的有效范围内
            return x >= 0 && x < bagSize.x && y >= 0 && y < bagSize.y;
        }
    }
}