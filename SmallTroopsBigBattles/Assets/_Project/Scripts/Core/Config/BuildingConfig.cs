using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Config
{
    /// <summary>
    /// 建築配置
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "SmallTroopsBigBattles/Config/Building Config")]
    public class BuildingConfig : ScriptableObject
    {
        public List<BuildingConfigData> Buildings = new List<BuildingConfigData>();

        private Dictionary<TerritoryBuildingType, BuildingConfigData> _buildingDict;

        public BuildingConfigData GetConfig(TerritoryBuildingType type)
        {
            if (_buildingDict == null)
            {
                _buildingDict = new Dictionary<TerritoryBuildingType, BuildingConfigData>();
                foreach (var building in Buildings)
                {
                    _buildingDict[building.Type] = building;
                }
            }

            return _buildingDict.TryGetValue(type, out var config) ? config : null;
        }

        private static BuildingConfig _instance;
        public static BuildingConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<BuildingConfig>("Config/BuildingConfig");
                }
                return _instance;
            }
        }
    }

    [Serializable]
    public class BuildingConfigData
    {
        public TerritoryBuildingType Type;
        public string Name;
        public string Description;
        public int MaxLevel = 10;

        [Header("基礎成本")]
        public int BaseCopperCost;
        public int BaseWoodCost;
        public int BaseStoneCost;
        public int BaseFoodCost;

        [Header("成本成長")]
        public float CostGrowthRate = 0.5f;  // 每級成本增長率

        [Header("建造時間")]
        public int BaseConstructionTime = 60;  // 基礎建造時間 (秒)
        public float TimeGrowthRate = 0.3f;    // 每級時間增長率

        [Header("產出 (資源建築用)")]
        public ResourceType ProductionType;
        public int BaseProduction = 100;       // 基礎每小時產量
        public float ProductionGrowthRate = 0.2f;

        /// <summary>
        /// 計算建築成本
        /// </summary>
        public Dictionary<ResourceType, int> GetCost(int level)
        {
            float multiplier = 1 + (level - 1) * CostGrowthRate;
            var costs = new Dictionary<ResourceType, int>();

            if (BaseCopperCost > 0)
                costs[ResourceType.Copper] = Mathf.RoundToInt(BaseCopperCost * multiplier);
            if (BaseWoodCost > 0)
                costs[ResourceType.Wood] = Mathf.RoundToInt(BaseWoodCost * multiplier);
            if (BaseStoneCost > 0)
                costs[ResourceType.Stone] = Mathf.RoundToInt(BaseStoneCost * multiplier);
            if (BaseFoodCost > 0)
                costs[ResourceType.Food] = Mathf.RoundToInt(BaseFoodCost * multiplier);

            return costs;
        }

        /// <summary>
        /// 計算建造時間
        /// </summary>
        public int GetConstructionTime(int level)
        {
            return Mathf.RoundToInt(BaseConstructionTime * (1 + level * TimeGrowthRate));
        }

        /// <summary>
        /// 計算產量
        /// </summary>
        public int GetProduction(int level)
        {
            return Mathf.RoundToInt(BaseProduction * (1 + level * ProductionGrowthRate));
        }
    }
}
