using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Core.Managers
{
    /// <summary>
    /// 領地管理器 - 處理領地與建築相關邏輯
    /// </summary>
    public class TerritoryManager : Singleton<TerritoryManager>
    {
        // 玩家領地資料
        private Dictionary<int, TerritoryData> _territories = new Dictionary<int, TerritoryData>();

        // 建築佇列 (每個領地只能有一個建築在建造)
        private Dictionary<int, BuildingData> _constructionQueue = new Dictionary<int, BuildingData>();

        // 事件
        public event Action<int, BuildingData> OnBuildingConstructed;
        public event Action<int, BuildingData> OnBuildingUpgraded;
        public event Action<TerritoryData> OnTerritoryAdded;

        protected override void OnSingletonAwake()
        {
            // 初始化
        }

        private void Update()
        {
            ProcessConstructionQueue();
        }

        /// <summary>
        /// 處理建築佇列
        /// </summary>
        private void ProcessConstructionQueue()
        {
            var completedBuildings = new List<(int territoryId, BuildingData building)>();

            foreach (var kvp in _constructionQueue.ToList())
            {
                var building = kvp.Value;
                if (DateTime.Now >= building.ConstructionEndTime)
                {
                    completedBuildings.Add((kvp.Key, building));
                }
            }

            foreach (var (territoryId, building) in completedBuildings)
            {
                CompleteConstruction(territoryId, building);
            }
        }

        private void CompleteConstruction(int territoryId, BuildingData building)
        {
            building.IsConstructing = false;
            _constructionQueue.Remove(territoryId);

            Debug.Log($"[TerritoryManager] 建築完成: 領地{territoryId} - {building.Type} Lv.{building.Level}");

            OnBuildingConstructed?.Invoke(territoryId, building);
            EventManager.Instance.Publish(new BuildingConstructedEvent
            {
                TerritoryId = territoryId,
                Building = building
            });

            // 重新計算資源生產
            ResourceManager.Instance.RecalculateProductionRates();
        }

        /// <summary>
        /// 新增領地
        /// </summary>
        public bool AddTerritory(TerritoryData territory)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            if (player.TerritoryIds.Count >= PlayerData.MaxTerritories)
            {
                Debug.LogWarning("[TerritoryManager] 已達領地上限");
                return false;
            }

            _territories[territory.TerritoryId] = territory;
            player.TerritoryIds.Add(territory.TerritoryId);

            Debug.Log($"[TerritoryManager] 新增領地: {territory.TerritoryId}");
            OnTerritoryAdded?.Invoke(territory);

            return true;
        }

        /// <summary>
        /// 取得玩家所有領地
        /// </summary>
        public List<TerritoryData> GetPlayerTerritories()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return new List<TerritoryData>();

            return player.TerritoryIds
                .Where(id => _territories.ContainsKey(id))
                .Select(id => _territories[id])
                .ToList();
        }

        /// <summary>
        /// 取得特定領地
        /// </summary>
        public TerritoryData GetTerritory(int territoryId)
        {
            return _territories.TryGetValue(territoryId, out var territory) ? territory : null;
        }

        /// <summary>
        /// 建造建築
        /// </summary>
        public bool ConstructBuilding(int territoryId, int slotIndex, TerritoryBuildingType type)
        {
            var territory = GetTerritory(territoryId);
            if (territory == null)
            {
                Debug.LogWarning("[TerritoryManager] 領地不存在");
                return false;
            }

            // 檢查建築格是否可用
            int maxSlots = GetMaxBuildingSlots(territory);
            if (slotIndex >= maxSlots)
            {
                Debug.LogWarning("[TerritoryManager] 建築格未解鎖");
                return false;
            }

            // 檢查是否已有建築
            if (territory.Buildings.Any(b => b.SlotIndex == slotIndex))
            {
                Debug.LogWarning("[TerritoryManager] 該位置已有建築");
                return false;
            }

            // 檢查是否正在建造其他建築
            if (_constructionQueue.ContainsKey(territoryId))
            {
                Debug.LogWarning("[TerritoryManager] 該領地已有建築在建造中");
                return false;
            }

            // 檢查並消耗資源
            var costs = GetBuildingCost(type, 1);
            if (!ResourceManager.Instance.ConsumeResources(costs))
            {
                Debug.LogWarning("[TerritoryManager] 資源不足");
                return false;
            }

            // 建立建築
            var building = new BuildingData
            {
                SlotIndex = slotIndex,
                Type = type,
                Level = 1,
                IsConstructing = true,
                ConstructionEndTime = DateTime.Now.AddSeconds(GetConstructionTime(type, 1))
            };

            territory.Buildings.Add(building);
            _constructionQueue[territoryId] = building;

            Debug.Log($"[TerritoryManager] 開始建造: {type} at 領地{territoryId}");
            return true;
        }

        /// <summary>
        /// 升級建築
        /// </summary>
        public bool UpgradeBuilding(int territoryId, int slotIndex)
        {
            var territory = GetTerritory(territoryId);
            if (territory == null) return false;

            var building = territory.Buildings.FirstOrDefault(b => b.SlotIndex == slotIndex);
            if (building == null)
            {
                Debug.LogWarning("[TerritoryManager] 找不到建築");
                return false;
            }

            if (building.IsConstructing)
            {
                Debug.LogWarning("[TerritoryManager] 建築正在建造中");
                return false;
            }

            if (_constructionQueue.ContainsKey(territoryId))
            {
                Debug.LogWarning("[TerritoryManager] 該領地已有建築在建造中");
                return false;
            }

            int newLevel = building.Level + 1;
            var costs = GetBuildingCost(building.Type, newLevel);
            if (!ResourceManager.Instance.ConsumeResources(costs))
            {
                Debug.LogWarning("[TerritoryManager] 資源不足");
                return false;
            }

            building.Level = newLevel;
            building.IsConstructing = true;
            building.ConstructionEndTime = DateTime.Now.AddSeconds(GetConstructionTime(building.Type, newLevel));
            _constructionQueue[territoryId] = building;

            Debug.Log($"[TerritoryManager] 開始升級: {building.Type} to Lv.{newLevel}");
            return true;
        }

        /// <summary>
        /// 取得領地最大建築格數
        /// </summary>
        public int GetMaxBuildingSlots(TerritoryData territory)
        {
            // 基礎15格，科技可增加至20格
            // TODO: 加入科技加成
            return TerritoryData.BaseBuildingSlots;
        }

        /// <summary>
        /// 取得建築費用
        /// </summary>
        public Dictionary<ResourceType, int> GetBuildingCost(TerritoryBuildingType type, int level)
        {
            // 基礎費用 * 等級倍率
            float multiplier = 1 + (level - 1) * 0.5f;

            var baseCosts = GetBaseBuildingCost(type);
            var result = new Dictionary<ResourceType, int>();

            foreach (var cost in baseCosts)
            {
                result[cost.Key] = Mathf.RoundToInt(cost.Value * multiplier);
            }

            return result;
        }

        private Dictionary<ResourceType, int> GetBaseBuildingCost(TerritoryBuildingType type)
        {
            return type switch
            {
                TerritoryBuildingType.Farm => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 100 },
                    { ResourceType.Stone, 50 }
                },
                TerritoryBuildingType.Lumberyard => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 50 },
                    { ResourceType.Stone, 100 }
                },
                TerritoryBuildingType.Quarry => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 80 },
                    { ResourceType.Stone, 80 },
                    { ResourceType.Copper, 50 }
                },
                TerritoryBuildingType.Barracks => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 200 },
                    { ResourceType.Stone, 150 },
                    { ResourceType.Copper, 100 }
                },
                TerritoryBuildingType.Market => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 150 },
                    { ResourceType.Stone, 100 },
                    { ResourceType.Copper, 200 }
                },
                TerritoryBuildingType.Warehouse => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 120 },
                    { ResourceType.Stone, 120 }
                },
                TerritoryBuildingType.Academy => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 300 },
                    { ResourceType.Stone, 200 },
                    { ResourceType.Copper, 500 }
                },
                _ => new Dictionary<ResourceType, int>()
            };
        }

        /// <summary>
        /// 取得建築時間 (秒)
        /// </summary>
        public int GetConstructionTime(TerritoryBuildingType type, int level)
        {
            int baseTime = type switch
            {
                TerritoryBuildingType.Farm => 60,
                TerritoryBuildingType.Lumberyard => 60,
                TerritoryBuildingType.Quarry => 90,
                TerritoryBuildingType.Barracks => 120,
                TerritoryBuildingType.Market => 90,
                TerritoryBuildingType.Warehouse => 60,
                TerritoryBuildingType.Academy => 180,
                _ => 60
            };

            // 時間 = 基礎時間 * (1 + 等級 * 0.3)
            return Mathf.RoundToInt(baseTime * (1 + level * 0.3f));
        }

        /// <summary>
        /// 取得建築佇列剩餘時間
        /// </summary>
        public float GetConstructionRemainingTime(int territoryId)
        {
            if (!_constructionQueue.TryGetValue(territoryId, out var building))
                return 0;

            return (float)(building.ConstructionEndTime - DateTime.Now).TotalSeconds;
        }
    }
}
