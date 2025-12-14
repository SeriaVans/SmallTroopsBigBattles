using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Data
{
    #region 玩家相關

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

        // 資源
        public PlayerResources Resources;

        // 軍隊
        public PlayerArmy Army;

        // 領地列表
        public List<int> TerritoryIds = new List<int>();
        public const int MaxTerritories = 3;
    }

    /// <summary>
    /// 玩家資源池 (跨領地共用)
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        public int Copper;      // 銅錢
        public int Wood;        // 木材
        public int Stone;       // 石材
        public int Food;        // 糧草

        // 資源上限
        public int MaxCopper = 10000;
        public int MaxWood = 10000;
        public int MaxStone = 10000;
        public int MaxFood = 10000;

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

        public void SetResource(ResourceType type, int value)
        {
            switch (type)
            {
                case ResourceType.Copper: Copper = Mathf.Clamp(value, 0, MaxCopper); break;
                case ResourceType.Wood: Wood = Mathf.Clamp(value, 0, MaxWood); break;
                case ResourceType.Stone: Stone = Mathf.Clamp(value, 0, MaxStone); break;
                case ResourceType.Food: Food = Mathf.Clamp(value, 0, MaxFood); break;
            }
        }

        public void AddResource(ResourceType type, int amount)
        {
            SetResource(type, GetResource(type) + amount);
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
        public const int MaxSoldiers = 5000;

        // 各兵種數量
        public int Spearman;    // 槍兵
        public int Shieldman;   // 盾兵
        public int Cavalry;     // 騎兵
        public int Archer;      // 弓兵

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
            switch (type)
            {
                case SoldierType.Spearman: Spearman = Mathf.Max(0, count); break;
                case SoldierType.Shieldman: Shieldman = Mathf.Max(0, count); break;
                case SoldierType.Cavalry: Cavalry = Mathf.Max(0, count); break;
                case SoldierType.Archer: Archer = Mathf.Max(0, count); break;
            }
        }

        public bool CanAddSoldiers(int count)
        {
            return TotalSoldiers + count <= MaxSoldiers;
        }
    }

    #endregion

    #region 領地相關

    /// <summary>
    /// 領地資料
    /// </summary>
    [Serializable]
    public class TerritoryData
    {
        public int TerritoryId;
        public long PlayerId;
        public int CityId;              // 所屬城池
        public int TerritoryLevel;      // 領地等級

        // 建築
        public const int BaseBuildingSlots = 15;    // 基礎建築格數
        public const int MaxBuildingSlots = 20;     // 最大建築格數 (科技+5)
        public List<BuildingData> Buildings = new List<BuildingData>();

        public int StationedSoldiers;   // 該領地囤放的士兵數
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

    #endregion

    #region 地圖相關

    /// <summary>
    /// 地圖節點
    /// </summary>
    [Serializable]
    public class MapNodeData
    {
        public int NodeId;
        public string Name;
        public NodeType Type;
        public int NationId;            // 所屬國家 (0=中立)
        public Vector2 Position;        // 地圖座標 (顯示用)
        public int DefenseLevel;        // 防禦等級
        public List<int> ConnectedNodes = new List<int>(); // 連接的節點ID
    }

    /// <summary>
    /// 路線資料
    /// </summary>
    [Serializable]
    public class MapRouteData
    {
        public int RouteId;
        public int FromNodeId;
        public int ToNodeId;
        public int Distance;
        public TerrainType Terrain;
        public bool IsTwoWay = true;
    }

    /// <summary>
    /// 城池資料
    /// </summary>
    [Serializable]
    public class CityData
    {
        public int CityId;
        public string Name;
        public int NationId;
        public int CityLevel;
        public int WallHp;
        public int WallMaxHp;
        public int WallDefense;
        public List<CityFacilityData> Facilities = new List<CityFacilityData>();
        public List<long> ResidentPlayers = new List<long>();
        public long GovernorId;
        public const int MaxResidents = 100;
    }

    /// <summary>
    /// 城池設施資料
    /// </summary>
    [Serializable]
    public class CityFacilityData
    {
        public CityFacilityType Type;
        public int Level;
    }

    #endregion

    #region 將領相關

    /// <summary>
    /// 將領資料
    /// </summary>
    [Serializable]
    public class GeneralData
    {
        public long GeneralId;
        public string Name;
        public int Rarity;              // 稀有度: 1-5星
        public GeneralClass Class;

        // 等級
        public int Level = 1;
        public const int MaxLevel = 50;
        public int Exp;

        // 四維屬性
        public float Strength;          // 武力
        public float Intelligence;      // 智力
        public float Command;           // 統帥
        public float Speed;             // 速度

        /// <summary>
        /// 計算帶兵上限
        /// </summary>
        public int GetMaxTroops()
        {
            return 500 + (int)(Command * 10);
        }

        /// <summary>
        /// 取得擅長兵種
        /// </summary>
        public SoldierType[] GetProficientSoldiers()
        {
            return Class switch
            {
                GeneralClass.Commander => new[] { SoldierType.Spearman, SoldierType.Shieldman },
                GeneralClass.Vanguard => new[] { SoldierType.Cavalry },
                GeneralClass.Strategist => new[] { SoldierType.Archer },
                _ => Array.Empty<SoldierType>()
            };
        }
    }

    #endregion

    #region 國家相關

    /// <summary>
    /// 國家資料
    /// </summary>
    [Serializable]
    public class NationData
    {
        public int NationId;
        public string Name;
        public Color NationColor;
        public long KingPlayerId;
        public List<int> OwnedCityIds = new List<int>();
        public int TotalPlayers;
    }

    #endregion
}
