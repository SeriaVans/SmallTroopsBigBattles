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
        private Dictionary<int, TerritoryData> _territories = new Dictionary<int, TerritoryData>();
        private Dictionary<int, BuildingData> _constructionQueue = new Dictionary<int, BuildingData>();

        public event Action<int, BuildingData> OnBuildingConstructed;
        public event Action<TerritoryData> OnTerritoryAdded;

        private void Update()
        {
            ProcessConstructionQueue();
        }

        private void ProcessConstructionQueue()
        {
            var completed = new List<(int, BuildingData)>();
            foreach (var kvp in _constructionQueue.ToList())
            {
                if (DateTime.Now >= kvp.Value.ConstructionEndTime)
                    completed.Add((kvp.Key, kvp.Value));
            }
            foreach (var (territoryId, building) in completed)
                CompleteConstruction(territoryId, building);
        }

        private void CompleteConstruction(int territoryId, BuildingData building)
        {
            building.IsConstructing = false;
            _constructionQueue.Remove(territoryId);
            Debug.Log($"[TerritoryManager] 建築完成: {building.Type} Lv.{building.Level}");
            OnBuildingConstructed?.Invoke(territoryId, building);
            EventManager.Instance.Publish(new BuildingConstructedEvent { TerritoryId = territoryId, Building = building });
            ResourceManager.Instance.RecalculateProductionRates();
        }

        public bool AddTerritory(TerritoryData territory)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null || player.TerritoryIds.Count >= PlayerData.MaxTerritories) return false;

            _territories[territory.TerritoryId] = territory;
            player.TerritoryIds.Add(territory.TerritoryId);
            OnTerritoryAdded?.Invoke(territory);
            return true;
        }

        public List<TerritoryData> GetPlayerTerritories()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return new List<TerritoryData>();
            return player.TerritoryIds.Where(id => _territories.ContainsKey(id)).Select(id => _territories[id]).ToList();
        }

        public TerritoryData GetTerritory(int territoryId)
        {
            return _territories.TryGetValue(territoryId, out var t) ? t : null;
        }

        public bool ConstructBuilding(int territoryId, int slotIndex, TerritoryBuildingType type)
        {
            var territory = GetTerritory(territoryId);
            if (territory == null) return false;
            if (slotIndex >= GetMaxBuildingSlots(territory)) return false;
            if (territory.Buildings.Any(b => b.SlotIndex == slotIndex)) return false;
            if (_constructionQueue.ContainsKey(territoryId)) return false;

            var costs = GetBuildingCost(type, 1);
            if (!ResourceManager.Instance.ConsumeResources(costs)) return false;

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
            return true;
        }

        public bool UpgradeBuilding(int territoryId, int slotIndex)
        {
            var territory = GetTerritory(territoryId);
            if (territory == null) return false;

            var building = territory.Buildings.FirstOrDefault(b => b.SlotIndex == slotIndex);
            if (building == null || building.IsConstructing) return false;
            if (_constructionQueue.ContainsKey(territoryId)) return false;

            int newLevel = building.Level + 1;
            var costs = GetBuildingCost(building.Type, newLevel);
            if (!ResourceManager.Instance.ConsumeResources(costs)) return false;

            building.Level = newLevel;
            building.IsConstructing = true;
            building.ConstructionEndTime = DateTime.Now.AddSeconds(GetConstructionTime(building.Type, newLevel));
            _constructionQueue[territoryId] = building;
            return true;
        }

        public int GetMaxBuildingSlots(TerritoryData territory) => TerritoryData.BaseBuildingSlots;

        public Dictionary<ResourceType, int> GetBuildingCost(TerritoryBuildingType type, int level)
        {
            float multiplier = 1 + (level - 1) * 0.5f;
            var baseCosts = GetBaseBuildingCost(type);
            return baseCosts.ToDictionary(c => c.Key, c => Mathf.RoundToInt(c.Value * multiplier));
        }

        private Dictionary<ResourceType, int> GetBaseBuildingCost(TerritoryBuildingType type)
        {
            return type switch
            {
                TerritoryBuildingType.Farm => new Dictionary<ResourceType, int> { { ResourceType.Wood, 100 }, { ResourceType.Stone, 50 } },
                TerritoryBuildingType.Lumberyard => new Dictionary<ResourceType, int> { { ResourceType.Wood, 50 }, { ResourceType.Stone, 100 } },
                TerritoryBuildingType.Quarry => new Dictionary<ResourceType, int> { { ResourceType.Wood, 80 }, { ResourceType.Stone, 80 }, { ResourceType.Copper, 50 } },
                TerritoryBuildingType.Barracks => new Dictionary<ResourceType, int> { { ResourceType.Wood, 200 }, { ResourceType.Stone, 150 }, { ResourceType.Copper, 100 } },
                TerritoryBuildingType.Market => new Dictionary<ResourceType, int> { { ResourceType.Wood, 150 }, { ResourceType.Stone, 100 }, { ResourceType.Copper, 200 } },
                TerritoryBuildingType.Warehouse => new Dictionary<ResourceType, int> { { ResourceType.Wood, 120 }, { ResourceType.Stone, 120 } },
                TerritoryBuildingType.Academy => new Dictionary<ResourceType, int> { { ResourceType.Wood, 300 }, { ResourceType.Stone, 200 }, { ResourceType.Copper, 500 } },
                _ => new Dictionary<ResourceType, int>()
            };
        }

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
            return Mathf.RoundToInt(baseTime * (1 + level * 0.3f));
        }

        public float GetConstructionRemainingTime(int territoryId)
        {
            if (!_constructionQueue.TryGetValue(territoryId, out var building)) return 0;
            return (float)(building.ConstructionEndTime - DateTime.Now).TotalSeconds;
        }
    }
}
