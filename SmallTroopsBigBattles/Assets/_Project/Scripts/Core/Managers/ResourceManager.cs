using System;
using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Core.Managers
{
    /// <summary>
    /// 資源管理器 - 處理資源生產與消耗
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        [Header("資源更新設定")]
        [SerializeField] private float _updateInterval = 1f; // 每秒更新
        private float _updateTimer;

        // 生產速率 (每小時)
        private Dictionary<ResourceType, int> _productionRates = new Dictionary<ResourceType, int>();

        // 事件
        public event Action<ResourceType, int, int> OnResourceChanged; // 類型, 舊值, 新值

        protected override void OnSingletonAwake()
        {
            InitializeProductionRates();
        }

        private void InitializeProductionRates()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _productionRates[type] = 0;
            }
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0;
                ProcessResourceProduction();
            }
        }

        /// <summary>
        /// 處理資源生產
        /// </summary>
        private void ProcessResourceProduction()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            // 計算每秒生產量 (小時產量 / 3600)
            foreach (var kvp in _productionRates)
            {
                if (kvp.Value > 0)
                {
                    float perSecond = kvp.Value / 3600f;
                    int amount = Mathf.FloorToInt(perSecond * _updateInterval);
                    if (amount > 0)
                    {
                        AddResource(kvp.Key, amount);
                    }
                }
            }
        }

        /// <summary>
        /// 設定資源生產速率 (每小時)
        /// </summary>
        public void SetProductionRate(ResourceType type, int ratePerHour)
        {
            _productionRates[type] = ratePerHour;
            Debug.Log($"[ResourceManager] {type} 生產速率設為 {ratePerHour}/小時");
        }

        /// <summary>
        /// 取得資源生產速率
        /// </summary>
        public int GetProductionRate(ResourceType type)
        {
            return _productionRates.TryGetValue(type, out int rate) ? rate : 0;
        }

        /// <summary>
        /// 增加資源
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            int oldValue = player.Resources.GetResource(type);
            player.Resources.AddResource(type, amount);
            int newValue = player.Resources.GetResource(type);

            if (oldValue != newValue)
            {
                OnResourceChanged?.Invoke(type, oldValue, newValue);
                EventManager.Instance.Publish(new ResourceChangedEvent
                {
                    Type = type,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Delta = newValue - oldValue
                });
            }
        }

        /// <summary>
        /// 消耗資源
        /// </summary>
        public bool ConsumeResource(ResourceType type, int amount)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            if (!player.Resources.HasEnough(type, amount))
            {
                Debug.LogWarning($"[ResourceManager] {type} 不足: 需要 {amount}, 擁有 {player.Resources.GetResource(type)}");
                return false;
            }

            AddResource(type, -amount);
            return true;
        }

        /// <summary>
        /// 消耗多種資源
        /// </summary>
        public bool ConsumeResources(Dictionary<ResourceType, int> costs)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            // 先檢查是否全部足夠
            foreach (var cost in costs)
            {
                if (!player.Resources.HasEnough(cost.Key, cost.Value))
                {
                    Debug.LogWarning($"[ResourceManager] {cost.Key} 不足");
                    return false;
                }
            }

            // 全部扣除
            foreach (var cost in costs)
            {
                AddResource(cost.Key, -cost.Value);
            }

            return true;
        }

        /// <summary>
        /// 取得當前資源
        /// </summary>
        public int GetResource(ResourceType type)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            return player?.Resources.GetResource(type) ?? 0;
        }

        /// <summary>
        /// 檢查是否有足夠資源
        /// </summary>
        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }

        /// <summary>
        /// 檢查是否有足夠多種資源
        /// </summary>
        public bool HasEnoughResources(Dictionary<ResourceType, int> costs)
        {
            foreach (var cost in costs)
            {
                if (!HasEnoughResource(cost.Key, cost.Value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 重新計算所有建築的生產速率
        /// </summary>
        public void RecalculateProductionRates()
        {
            // 重置所有生產速率
            InitializeProductionRates();

            // 從所有領地的建築計算總生產速率
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            var territories = TerritoryManager.Instance.GetPlayerTerritories();
            foreach (var territory in territories)
            {
                foreach (var building in territory.Buildings)
                {
                    AddBuildingProduction(building);
                }
            }
        }

        private void AddBuildingProduction(BuildingData building)
        {
            if (building.IsConstructing) return;

            // 根據建築類型和等級計算生產量
            int production = CalculateBuildingProduction(building.Type, building.Level);

            switch (building.Type)
            {
                case TerritoryBuildingType.Farm:
                    _productionRates[ResourceType.Food] += production;
                    break;
                case TerritoryBuildingType.Lumberyard:
                    _productionRates[ResourceType.Wood] += production;
                    break;
                case TerritoryBuildingType.Quarry:
                    _productionRates[ResourceType.Stone] += production;
                    break;
                case TerritoryBuildingType.Market:
                    _productionRates[ResourceType.Copper] += production;
                    break;
            }
        }

        private int CalculateBuildingProduction(TerritoryBuildingType type, int level)
        {
            // 基礎生產量公式: 基礎值 * (1 + 等級 * 0.2)
            int baseProduction = type switch
            {
                TerritoryBuildingType.Farm => 100,
                TerritoryBuildingType.Lumberyard => 80,
                TerritoryBuildingType.Quarry => 60,
                TerritoryBuildingType.Market => 50,
                _ => 0
            };

            return Mathf.RoundToInt(baseProduction * (1 + level * 0.2f));
        }
    }
}
