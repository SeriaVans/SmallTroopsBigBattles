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
        [SerializeField] private float _updateInterval = 1f;
        private float _updateTimer;

        private Dictionary<ResourceType, int> _productionRates = new Dictionary<ResourceType, int>();

        public event Action<ResourceType, int, int> OnResourceChanged;

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

        private void ProcessResourceProduction()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            foreach (var kvp in _productionRates)
            {
                if (kvp.Value > 0)
                {
                    float perSecond = kvp.Value / 3600f;
                    int amount = Mathf.FloorToInt(perSecond * _updateInterval);
                    if (amount > 0) AddResource(kvp.Key, amount);
                }
            }
        }

        public void SetProductionRate(ResourceType type, int ratePerHour)
        {
            _productionRates[type] = ratePerHour;
        }

        public int GetProductionRate(ResourceType type)
        {
            return _productionRates.TryGetValue(type, out int rate) ? rate : 0;
        }

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
                    Type = type, OldValue = oldValue, NewValue = newValue, Delta = newValue - oldValue
                });
            }
        }

        public bool ConsumeResource(ResourceType type, int amount)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;
            if (!player.Resources.HasEnough(type, amount)) return false;
            AddResource(type, -amount);
            return true;
        }

        public bool ConsumeResources(Dictionary<ResourceType, int> costs)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            foreach (var cost in costs)
            {
                if (!player.Resources.HasEnough(cost.Key, cost.Value)) return false;
            }

            foreach (var cost in costs)
            {
                AddResource(cost.Key, -cost.Value);
            }
            return true;
        }

        public int GetResource(ResourceType type)
        {
            return GameManager.Instance?.CurrentPlayer?.Resources.GetResource(type) ?? 0;
        }

        public bool HasEnoughResource(ResourceType type, int amount) => GetResource(type) >= amount;

        public bool HasEnoughResources(Dictionary<ResourceType, int> costs)
        {
            foreach (var cost in costs)
            {
                if (!HasEnoughResource(cost.Key, cost.Value)) return false;
            }
            return true;
        }

        public void RecalculateProductionRates()
        {
            InitializeProductionRates();
            var territories = TerritoryManager.Instance.GetPlayerTerritories();
            foreach (var territory in territories)
            {
                foreach (var building in territory.Buildings)
                {
                    if (!building.IsConstructing) AddBuildingProduction(building);
                }
            }
        }

        private void AddBuildingProduction(BuildingData building)
        {
            int production = CalculateBuildingProduction(building.Type, building.Level);
            switch (building.Type)
            {
                case TerritoryBuildingType.Farm: _productionRates[ResourceType.Food] += production; break;
                case TerritoryBuildingType.Lumberyard: _productionRates[ResourceType.Wood] += production; break;
                case TerritoryBuildingType.Quarry: _productionRates[ResourceType.Stone] += production; break;
                case TerritoryBuildingType.Market: _productionRates[ResourceType.Copper] += production; break;
            }
        }

        private int CalculateBuildingProduction(TerritoryBuildingType type, int level)
        {
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
