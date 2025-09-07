using System.Collections.Generic;
using CommonBase;
using UnityEngine;

namespace FishCollection
{
    public class BagSystem : Singleton<BagSystem>
    {
        public Vector2Int bagSize;
        public Dictionary<int, SpecialFishCaught> fishDic;
        private int[,] bagOccupancy;

        private BagSystem()
        {
            bagSize = new Vector2Int(7, 6);
            fishDic = new Dictionary<int, SpecialFishCaught>();
            bagOccupancy = new int[bagSize.x, bagSize.y];
            for (int x = 0; x < bagSize.x; x++)
            for (int y = 0; y < bagSize.y; y++)
                bagOccupancy[x, y] = -1;

            //注册事件
            GameManager.Instance.RegisterOnFishEaten(AddFish);
        }

        public override void Dispose()
        {
            base.Dispose();
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
    }
}