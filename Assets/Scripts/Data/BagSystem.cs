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
        private int occupancyCount;
        private static readonly Vector2Int InvalidPos = new Vector2Int(-1, -1);
        
        [SerializedDictionary("fishId", "Info")]
        public SerializedDictionary<int, SpecialFishCaught> fishDic;

        public int[,] bagOccupancy;

        private void Start()
        {
            occupancyCount = 0;
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
            GameManager.Instance?.UnregisterOnFishEaten(AddFish);
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
                        MarkOccupied(fish, x, y, bagOccupancy);

                        // 将鱼添加到列表中
                        fishDic.Add(fish.specialFishId, new SpecialFishCaught(fish, new Vector2Int(x, y)));
                        this.EventTrigger(GameEvent.BagChange);
                        occupancyCount += fish.InventoryGridSize.x * fish.InventoryGridSize.y;
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
            occupancyCount -= fish.inventorySize.x * fish.inventorySize.y;
            this.EventTrigger(GameEvent.BagChange);
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

        private void ClearOccupiedArea(SpecialFishCaught fish)
        {
            for (int x = fish.topLeftPos.x; x < fish.inventorySize.x + fish.topLeftPos.x; x++)
            {
                for (int y = fish.topLeftPos.y; y < fish.inventorySize.y + fish.topLeftPos.y; y++)
                {
                    bagOccupancy[x, y] = -1;
                }
            }
        }

        private void ClearOccupiedArea(SpecialFishCaught fish, int[,] bag)
        {
            for (int x = fish.topLeftPos.x; x < fish.inventorySize.x + fish.topLeftPos.x; x++)
            {
                for (int y = fish.topLeftPos.y; y < fish.inventorySize.y + fish.topLeftPos.y; y++)
                {
                    bag[x, y] = -1;
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

        private bool CanPlaceFishAt(SpecialFishCaught fish, int startX, int startY, List<int> excluded)
        {
            Vector2Int gridSize = fish.inventorySize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    if (x >= bagSize.x || y >= bagSize.y ||
                        (bagOccupancy[x, y] != -1 && bagOccupancy[x, y] != fish.fishId &&
                         !excluded.Contains(bagOccupancy[x, y])))
                    {
                        Debug.LogWarning("已占用");
                        return false; // 超出边界或已被占用
                    }
                }
            }

            return true; // 可以放置
        }

        private bool CanPlaceFishAtSuppose(SpecialFishCaught fish, int startX, int startY, SpecialFishCaught fishSwap)
        {
            int[,] bagOccupancyCopy = ArrayExtension.CopyArray(bagOccupancy);
            ClearOccupiedArea(fish, bagOccupancyCopy);
            ClearOccupiedArea(fishSwap, bagOccupancyCopy);

            MarkOccupied(fishSwap, fish.topLeftPos.x, fish.topLeftPos.y, bagOccupancyCopy);
            Vector2Int gridSize = fish.inventorySize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    if (x >= bagSize.x || y >= bagSize.y ||
                        (bagOccupancyCopy[x, y] != -1 && bagOccupancyCopy[x, y] != fish.fishId))
                    {
                        Debug.LogWarning("已占用");
                        return false; // 超出边界或已被占用
                    }
                }
            }

            return true; // 可以放置
        }

        private void MarkOccupied(SpecialFish fish, int startX, int startY, int[,] bag)
        {
            int index = fish.specialFishId;
            Vector2Int gridSize = fish.InventoryGridSize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    bag[x, y] = index;
                }
            }
        }

        private void MarkOccupied(SpecialFishCaught fish, int startX, int startY, int[,] bag)
        {
            int index = fish.fishId;
            Vector2Int gridSize = fish.inventorySize;
            for (int x = startX; x < startX + gridSize.x; x++)
            {
                for (int y = startY; y < startY + gridSize.y; y++)
                {
                    bag[x, y] = index;
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

        public bool CanFitFish(SpecialFishCaught fish, Vector2 targetGridPosition, List<int> excluded)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;
            return CanPlaceFishAt(fish, startX, startY, excluded);
        }

        public bool CanFitFishSuppose(SpecialFishCaught fish, Vector2 targetGridPosition, SpecialFishCaught fishToSwap)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;
            return CanPlaceFishAtSuppose(fish, startX, startY, fishToSwap);
        }

        // 移动鱼到目标位置
        public void MoveFish(SpecialFishCaught fishToMove, Vector2 targetGridPosition)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;

            if (CanFitFish(fishToMove, targetGridPosition))
            {
                ClearOccupiedArea(fishToMove.topLeftPos, fishToMove.inventorySize);
                MarkOccupied(fishToMove, startX, startY, bagOccupancy);
                fishToMove.topLeftPos = new Vector2Int(startX, startY);
                this.EventTrigger(GameEvent.BagChange);
            }
        }

        private void MoveFishDirectly(SpecialFishCaught fishToMove, Vector2 targetGridPosition)
        {
            int startX = (int)targetGridPosition.x;
            int startY = (int)targetGridPosition.y;

            MarkOccupied(fishToMove, startX, startY, bagOccupancy);
            fishToMove.topLeftPos = new Vector2Int(startX, startY);
        }

        // 交换两条鱼的位置
        public void SwapFish(SpecialFishCaught fishToMove, SpecialFishCaught fishAtTarget, Vector2Int targetPos)
        {
            if (CanFitFishSuppose(fishToMove, fishAtTarget.topLeftPos, fishAtTarget)
                && CanFitFishSuppose(fishAtTarget, fishToMove.topLeftPos, fishToMove))
            {
                Vector2Int tempPosition = fishToMove.topLeftPos;
                ClearOccupiedArea(fishToMove.topLeftPos, fishToMove.inventorySize);
                ClearOccupiedArea(fishAtTarget.topLeftPos, fishAtTarget.inventorySize);
                MoveFishDirectly(fishToMove, targetPos);
                MoveFishDirectly(fishAtTarget, tempPosition);
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

        public int GetCapcity()
        {
            return bagOccupancy.Length;
        }

        public int GetOccupancy()
        {
            return occupancyCount;
        }

        // 整理背包：双阶段算法
        public void OrganizeBag()
        {
            // 1. 收集所有鱼
            List<SpecialFishCaught> fishes = new List<SpecialFishCaught>(fishDic.Values);

            // 2. 清空背包
            fishDic.Clear();
            for (int x = 0; x < bagSize.x; x++)
            {
                for (int y = 0; y < bagSize.y; y++)
                {
                    bagOccupancy[x, y] = -1;
                }
            }

            occupancyCount = 0;

            if (fishes.Count == 0)
                return;

            // 3. 排序
            fishes.Sort((a, b) =>
            {
                int areaA = a.inventorySize.x * a.inventorySize.y;
                int areaB = b.inventorySize.x * b.inventorySize.y;
                return areaB.CompareTo(areaA); // 面积大 → 小
            });

            // 4. 阶段1：先放大件（取前70%）
            int thresholdIndex = Mathf.CeilToInt(fishes.Count * 0.7f);
            List<SpecialFishCaught> largeFishes = fishes.GetRange(0, thresholdIndex);
            List<SpecialFishCaught> smallFishes = fishes.GetRange(thresholdIndex, fishes.Count - thresholdIndex);

            foreach (var fish in largeFishes)
            {
                Vector2Int pos = FindBestFitPosition(fish); // 靠左上
                if (pos != InvalidPos)
                {
                    PlaceFish(fish, pos);
                }
                else
                {
                    Debug.LogWarning($"整理时无法放置大件鱼 {fish.fishId}");
                }
            }

            // 5. 阶段2：小件补洞（扫描优先补“靠近已有占用”的空位）
            foreach (var fish in smallFishes)
            {
                Vector2Int pos = FindBestFitPositionForSmall(fish);
                if (pos != Vector2Int.zero)
                {
                    PlaceFish(fish, pos);
                }
                else
                {
                    Debug.LogWarning($"整理时无法放置小件鱼 {fish.fishId}");
                }
            }

            // 6. 通知更新
            this.EventTrigger(GameEvent.BagChange);
        }

        private void PlaceFish(SpecialFishCaught fish, Vector2Int pos)
        {
            MarkOccupied(fish, pos.x, pos.y, bagOccupancy);
            fish.topLeftPos = pos;
            fishDic.Add(fish.fishId, fish);
            occupancyCount += fish.inventorySize.x * fish.inventorySize.y;
        }

        private Vector2Int FindBestFitPosition(SpecialFishCaught fish)
        {
            int bestFitX = -1;
            int bestFitY = -1;
            int bestFitScore = int.MaxValue;

            for (int x = 0; x < bagSize.x; x++)
            {
                for (int y = 0; y < bagSize.y; y++)
                {
                    if (CanPlaceFishAt(fish, x, y))
                    {
                        int score = x * bagSize.y + y; // 行主序，越靠左上分数越小
                        if (score < bestFitScore)
                        {
                            bestFitX = x;
                            bestFitY = y;
                            bestFitScore = score;
                        }
                    }
                }
            }

            return bestFitX >= 0 && bestFitY >= 0 ? new Vector2Int(bestFitX, bestFitY) : InvalidPos;
        }

        // 小件补洞用：靠近已有占用 → 越紧凑分数越高
        private Vector2Int FindBestFitPositionForSmall(SpecialFishCaught fish)
        {
            int bestFitX = -1;
            int bestFitY = -1;
            int bestFitScore = int.MaxValue;

            for (int x = 0; x < bagSize.x; x++)
            {
                for (int y = 0; y < bagSize.y; y++)
                {
                    if (CanPlaceFishAt(fish, x, y))
                    {
                        int score = CountAdjacentOccupied(x, y, fish.inventorySize);
                        if (score < bestFitScore)
                        {
                            bestFitX = x;
                            bestFitY = y;
                            bestFitScore = score;
                        }
                    }
                }
            }

            return bestFitX >= 0 && bestFitY >= 0 ? new Vector2Int(bestFitX, bestFitY) : Vector2Int.zero;
        }

        // 计算某区域周围已有占用格子数 → 越多越优先放置（更紧凑）
        private int CountAdjacentOccupied(int startX, int startY, Vector2Int size)
        {
            int score = 0;
            for (int x = startX; x < startX + size.x; x++)
            {
                for (int y = startY; y < startY + size.y; y++)
                {
                    if (x > 0 && bagOccupancy[x - 1, y] != -1) score++;
                    if (y > 0 && bagOccupancy[x, y - 1] != -1) score++;
                    if (x + 1 < bagSize.x && bagOccupancy[x + 1, y] != -1) score++;
                    if (y + 1 < bagSize.y && bagOccupancy[x, y + 1] != -1) score++;
                }
            }

            return -score; // 邻居越多，分数越小，优先放置
        }
    }
}