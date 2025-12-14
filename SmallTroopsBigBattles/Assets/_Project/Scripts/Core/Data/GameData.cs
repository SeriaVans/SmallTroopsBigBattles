using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Data
{
    /// <summary>
    /// 玩家資料
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public long PlayerId;
        public string Name;
        public int NationId;
        public int Level;
        public DateTime JoinTime;

        public PlayerResources Resources;
        public PlayerArmy Army;
        public List<int> TerritoryIds = new List<int>();
        public List<long> GeneralIds = new List<long>();

        public const int MaxTerritories = 3;
    }

    /// <summary>
    /// 玩家資源
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        public int Copper;
        public int Wood;
        public int Stone;
        public int Food;

        public int MaxCopper = 100000;
        public int MaxWood = 100000;
        public int MaxStone = 100000;
        public int MaxFood = 100000;

        public int GetResource(ResourceType type)
        {
            return type switch
            {
                ResourceType.Copper => Copper,
                ResourceType.Wood => Wood,
                ResourceType.Stone => Stone,
                ResourceType.Food => Food,
                _ => 0
            };
        }

        public void AddResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Copper:
                    Copper = Mathf.Clamp(Copper + amount, 0, MaxCopper);
                    break;
                case ResourceType.Wood:
                    Wood = Mathf.Clamp(Wood + amount, 0, MaxWood);
                    break;
                case ResourceType.Stone:
                    Stone = Mathf.Clamp(Stone + amount, 0, MaxStone);
                    break;
                case ResourceType.Food:
                    Food = Mathf.Clamp(Food + amount, 0, MaxFood);
                    break;
            }
        }

        public bool HasEnough(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }
    }

    /// <summary>
    /// 玩家軍隊
    /// </summary>
    [Serializable]
    public class PlayerArmy
    {
        public int Spearman;
        public int Shieldman;
        public int Cavalry;
        public int Archer;

        public const int MaxSoldiers = 5000;

        public int TotalSoldiers => Spearman + Shieldman + Cavalry + Archer;

        public int GetSoldierCount(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => Spearman,
                SoldierType.Shieldman => Shieldman,
                SoldierType.Cavalry => Cavalry,
                SoldierType.Archer => Archer,
                _ => 0
            };
        }

        public void SetSoldierCount(SoldierType type, int count)
        {
            count = Mathf.Max(0, count);
            switch (type)
            {
                case SoldierType.Spearman: Spearman = count; break;
                case SoldierType.Shieldman: Shieldman = count; break;
                case SoldierType.Cavalry: Cavalry = count; break;
                case SoldierType.Archer: Archer = count; break;
            }
        }

        public bool CanAddSoldiers(int count)
        {
            return TotalSoldiers + count <= MaxSoldiers;
        }
    }

    /// <summary>
    /// 領地資料
    /// </summary>
    [Serializable]
    public class TerritoryData
    {
        public int TerritoryId;
        public long PlayerId;
        public int CityId;
        public int TerritoryLevel;
        public List<BuildingData> Buildings = new List<BuildingData>();

        public const int BaseBuildingSlots = 15;
        public const int MaxBuildingSlots = 20;
    }

    /// <summary>
    /// 建築資料
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        public int SlotIndex;
        public TerritoryBuildingType Type;
        public int Level;
        public bool IsConstructing;
        public DateTime ConstructionEndTime;
    }

    /// <summary>
    /// 地圖節點資料
    /// </summary>
    [Serializable]
    public class MapNodeData
    {
        public int NodeId;
        public string Name;
        public NodeType Type;
        public TerrainType Terrain;
        public Vector2 Position;
        public int OwnerNationId;
        public List<int> ConnectedNodeIds = new List<int>();
    }

    /// <summary>
    /// 地圖路線資料
    /// </summary>
    [Serializable]
    public class MapRouteData
    {
        public int RouteId;
        public int FromNodeId;
        public int ToNodeId;
        public float Distance;
        public TerrainType Terrain;
    }

    /// <summary>
    /// 城市資料
    /// </summary>
    [Serializable]
    public class CityData
    {
        public int CityId;
        public string Name;
        public int NodeId;
        public int OwnerNationId;
        public int CityLevel;
        public int DefenseValue;
        public List<CityFacilityData> Facilities = new List<CityFacilityData>();
    }

    /// <summary>
    /// 城市設施資料
    /// </summary>
    [Serializable]
    public class CityFacilityData
    {
        public CityFacilityType Type;
        public int Level;
    }

    /// <summary>
    /// 將領資料
    /// </summary>
    [Serializable]
    public class GeneralData
    {
        public long GeneralId;
        public string Name;
        public int Rarity;          // 1-5 星
        public GeneralClass Class;
        public int Level;
        public int Exp;

        // 屬性
        public float Strength;      // 武力
        public float Intelligence;  // 智力
        public float Command;       // 統帥
        public float Speed;         // 速度

        public const int MaxLevel = 50;

        /// <summary>
        /// 計算帶兵上限
        /// </summary>
        public int GetMaxTroops()
        {
            return Mathf.RoundToInt(100 + Command * 10 + Level * 5);
        }
    }

    /// <summary>
    /// 國家資料
    /// </summary>
    [Serializable]
    public class NationData
    {
        public int NationId;
        public string Name;
        public int KingPlayerId;
        public int CapitalCityId;
        public List<int> CityIds = new List<int>();
        public List<long> MemberPlayerIds = new List<long>();
        public int NationLevel;
        public int NationExp;
    }
}
