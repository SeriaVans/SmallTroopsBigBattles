using System;
using System.Collections.Generic;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Data
{
    /// <summary>
    /// 玩家領地集合
    /// </summary>
    [Serializable]
    public class PlayerTerritories
    {
        public long PlayerId;

        /// <summary>
        /// 最多可擁有的領地數量
        /// </summary>
        public const int MaxTerritories = 3;

        /// <summary>
        /// 領地列表
        /// </summary>
        public List<TerritoryData> Territories = new();

        /// <summary>
        /// 當前領地數量
        /// </summary>
        public int TerritoryCount => Territories.Count;

        /// <summary>
        /// 是否可以建立新領地
        /// </summary>
        public bool CanCreateNewTerritory => TerritoryCount < MaxTerritories;

        /// <summary>
        /// 建立新領地
        /// </summary>
        public TerritoryData CreateTerritory(int cityId)
        {
            if (!CanCreateNewTerritory) return null;

            var territory = new TerritoryData
            {
                TerritoryId = TerritoryCount + 1,
                PlayerId = PlayerId,
                CityId = cityId,
                TerritoryLevel = 1
            };

            // 初始化府邸
            territory.InitializeMansion();

            Territories.Add(territory);
            return territory;
        }

        /// <summary>
        /// 獲取指定領地
        /// </summary>
        public TerritoryData GetTerritory(int territoryId)
        {
            return Territories.Find(t => t.TerritoryId == territoryId);
        }
    }

    /// <summary>
    /// 單個領地數據
    /// </summary>
    [Serializable]
    public class TerritoryData
    {
        public int TerritoryId;
        public long PlayerId;
        public int CityId;              // 所屬城池
        public int TerritoryLevel;      // 領地等級

        /// <summary>
        /// 基礎建築格數
        /// </summary>
        public const int BaseBuildingSlots = 15;

        /// <summary>
        /// 最大建築格數（科技可提升）
        /// </summary>
        public const int MaxBuildingSlots = 20;

        /// <summary>
        /// 府邸（預設建築，不可移除）
        /// </summary>
        public BuildingData Mansion;

        /// <summary>
        /// 其他建築列表
        /// </summary>
        public List<BuildingData> Buildings = new();

        /// <summary>
        /// 當前可用的建築格數
        /// </summary>
        public int AvailableBuildingSlots = BaseBuildingSlots;

        /// <summary>
        /// 已使用的建築格數
        /// </summary>
        public int UsedBuildingSlots => Buildings.Count;

        /// <summary>
        /// 剩餘空位
        /// </summary>
        public int EmptySlots => AvailableBuildingSlots - UsedBuildingSlots;

        /// <summary>
        /// 駐守在此領地的士兵數量
        /// </summary>
        public int StationedSoldiers;

        /// <summary>
        /// 初始化府邸
        /// </summary>
        public void InitializeMansion()
        {
            Mansion = new BuildingData
            {
                SlotIndex = -1,  // 府邸不佔用普通格位
                Type = BuildingType.Mansion,
                Level = 1
            };
        }

        /// <summary>
        /// 在指定格位建造建築
        /// </summary>
        public BuildingData BuildAt(int slotIndex, BuildingType type)
        {
            if (slotIndex < 0 || slotIndex >= AvailableBuildingSlots)
                return null;

            // 檢查格位是否已被佔用
            if (Buildings.Exists(b => b.SlotIndex == slotIndex))
                return null;

            var building = new BuildingData
            {
                SlotIndex = slotIndex,
                Type = type,
                Level = 1
            };

            Buildings.Add(building);
            return building;
        }

        /// <summary>
        /// 拆除指定格位的建築
        /// </summary>
        public bool DemolishAt(int slotIndex)
        {
            var building = Buildings.Find(b => b.SlotIndex == slotIndex);
            if (building == null) return false;

            Buildings.Remove(building);
            return true;
        }

        /// <summary>
        /// 獲取指定格位的建築
        /// </summary>
        public BuildingData GetBuildingAt(int slotIndex)
        {
            return Buildings.Find(b => b.SlotIndex == slotIndex);
        }

        /// <summary>
        /// 獲取指定類型的建築數量
        /// </summary>
        public int GetBuildingCount(BuildingType type)
        {
            return Buildings.FindAll(b => b.Type == type).Count;
        }

        /// <summary>
        /// 計算訓練速度加成
        /// </summary>
        public float GetTrainingSpeedBonus(BuildingType trainingBuilding)
        {
            const float SpeedBonusPerBuilding = 0.2f;  // 每個設施+20%速度
            int count = GetBuildingCount(trainingBuilding);
            return 1f + (count * SpeedBonusPerBuilding);
        }
    }

    /// <summary>
    /// 建築數據
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        public int SlotIndex;           // 格位索引
        public BuildingType Type;       // 建築類型
        public int Level;               // 建築等級
        public bool IsConstructing;     // 是否正在建造中
        public DateTime ConstructionEndTime;  // 建造完成時間

        /// <summary>
        /// 建築最大等級
        /// </summary>
        public const int MaxLevel = 10;

        /// <summary>
        /// 是否可以升級
        /// </summary>
        public bool CanUpgrade => Level < MaxLevel && !IsConstructing;

        /// <summary>
        /// 升級建築
        /// </summary>
        public void Upgrade()
        {
            if (CanUpgrade)
            {
                Level++;
            }
        }
    }
}

