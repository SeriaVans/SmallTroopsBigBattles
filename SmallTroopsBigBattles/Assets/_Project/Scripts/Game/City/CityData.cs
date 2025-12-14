using System;
using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Game.Data;

namespace SmallTroopsBigBattles.Game.City
{
    /// <summary>
    /// 城池等級
    /// </summary>
    public enum CityTier
    {
        Village = 1,    // 村莊
        Town = 2,       // 小鎮
        City = 3,       // 城市
        Fortress = 4,   // 堡壘
        Capital = 5     // 首都
    }

    /// <summary>
    /// 城池狀態
    /// </summary>
    public enum CityState
    {
        Normal,         // 正常
        UnderSiege,     // 被圍攻
        Damaged,        // 受損
        Developing      // 發展中
    }

    /// <summary>
    /// 城池設施類型
    /// </summary>
    public enum CityFacilityType
    {
        Barracks,       // 兵營 - 招募士兵
        Market,         // 市場 - 增加銅錢產出
        Granary,        // 糧倉 - 增加糧食儲存
        Wall,           // 城牆 - 增加防禦
        Workshop,       // 工坊 - 增加木材/石頭產出
        Academy,        // 學院 - 研究科技
        Stable,         // 馬廄 - 招募騎兵
        Arsenal         // 軍械庫 - 招募弓兵
    }

    /// <summary>
    /// 城池設施數據
    /// </summary>
    [Serializable]
    public class CityFacility
    {
        public CityFacilityType Type;
        public int Level;
        public bool IsUnderConstruction;
        public float ConstructionProgress;  // 0-1

        /// <summary>最大等級</summary>
        public int MaxLevel => 10;

        /// <summary>是否已達最高等級</summary>
        public bool IsMaxLevel => Level >= MaxLevel;

        /// <summary>
        /// 建構函式
        /// </summary>
        public CityFacility(CityFacilityType type, int level = 1)
        {
            Type = type;
            Level = level;
        }

        /// <summary>
        /// 獲取設施效果加成
        /// </summary>
        public float GetBonus()
        {
            return Level * GetBaseBonusPerLevel();
        }

        /// <summary>
        /// 獲取每級基礎加成
        /// </summary>
        private float GetBaseBonusPerLevel()
        {
            return Type switch
            {
                CityFacilityType.Barracks => 0.1f,   // 每級增加 10% 訓練速度
                CityFacilityType.Market => 0.15f,   // 每級增加 15% 銅錢產出
                CityFacilityType.Granary => 0.2f,   // 每級增加 20% 糧食儲存
                CityFacilityType.Wall => 0.1f,      // 每級增加 10% 防禦
                CityFacilityType.Workshop => 0.12f, // 每級增加 12% 資源產出
                CityFacilityType.Academy => 0.08f,  // 每級增加 8% 研究速度
                CityFacilityType.Stable => 0.1f,    // 每級增加 10% 騎兵訓練
                CityFacilityType.Arsenal => 0.1f,   // 每級增加 10% 弓兵訓練
                _ => 0.1f
            };
        }

        /// <summary>
        /// 獲取升級消耗
        /// </summary>
        public ResourceCost GetUpgradeCost()
        {
            int baseCost = Level * 500;
            return new ResourceCost
            {
                Copper = baseCost,
                Wood = baseCost / 2,
                Stone = baseCost / 3,
                Food = baseCost / 4
            };
        }
    }

    /// <summary>
    /// 資源消耗
    /// </summary>
    [Serializable]
    public class ResourceCost
    {
        public int Copper;
        public int Wood;
        public int Stone;
        public int Food;
    }

    /// <summary>
    /// 城池數據
    /// </summary>
    [Serializable]
    public class CityData
    {
        /// <summary>城池唯一 ID</summary>
        public string CityId;

        /// <summary>城池名稱</summary>
        public string CityName;

        /// <summary>所屬地圖節點 ID</summary>
        public string MapNodeId;

        /// <summary>控制國家 ID</summary>
        public string NationId;

        /// <summary>城池等級</summary>
        public CityTier Tier;

        /// <summary>城池狀態</summary>
        public CityState State;

        /// <summary>城池設施</summary>
        public List<CityFacility> Facilities = new List<CityFacility>();

        /// <summary>城池人口</summary>
        public int Population;

        /// <summary>城池防禦值</summary>
        public int Defense;

        /// <summary>城池耐久度（生命值）</summary>
        public int Durability;

        /// <summary>最大耐久度</summary>
        public int MaxDurability;

        /// <summary>駐守軍隊數量</summary>
        public int GarrisonCount;

        /// <summary>最大駐軍數量</summary>
        public int MaxGarrison;

        /// <summary>
        /// 建構函式
        /// </summary>
        public CityData()
        {
            CityId = Guid.NewGuid().ToString();
            Facilities = new List<CityFacility>();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public CityData(string name, CityTier tier)
        {
            CityId = Guid.NewGuid().ToString();
            CityName = name;
            Tier = tier;
            State = CityState.Normal;
            Facilities = new List<CityFacility>();

            InitializeByTier();
        }

        /// <summary>
        /// 根據城池等級初始化
        /// </summary>
        private void InitializeByTier()
        {
            switch (Tier)
            {
                case CityTier.Village:
                    Population = 1000;
                    Defense = 20;
                    MaxDurability = 5000;
                    MaxGarrison = 500;
                    break;

                case CityTier.Town:
                    Population = 5000;
                    Defense = 40;
                    MaxDurability = 10000;
                    MaxGarrison = 1500;
                    break;

                case CityTier.City:
                    Population = 20000;
                    Defense = 60;
                    MaxDurability = 25000;
                    MaxGarrison = 3000;
                    break;

                case CityTier.Fortress:
                    Population = 10000;
                    Defense = 100;
                    MaxDurability = 50000;
                    MaxGarrison = 5000;
                    break;

                case CityTier.Capital:
                    Population = 50000;
                    Defense = 80;
                    MaxDurability = 80000;
                    MaxGarrison = 10000;
                    break;
            }

            Durability = MaxDurability;

            // 添加基礎設施
            AddBasicFacilities();
        }

        /// <summary>
        /// 添加基礎設施
        /// </summary>
        private void AddBasicFacilities()
        {
            // 所有城池都有的基礎設施
            Facilities.Add(new CityFacility(CityFacilityType.Barracks, 1));
            Facilities.Add(new CityFacility(CityFacilityType.Wall, 1));

            if (Tier >= CityTier.Town)
            {
                Facilities.Add(new CityFacility(CityFacilityType.Market, 1));
                Facilities.Add(new CityFacility(CityFacilityType.Granary, 1));
            }

            if (Tier >= CityTier.City)
            {
                Facilities.Add(new CityFacility(CityFacilityType.Workshop, 1));
                Facilities.Add(new CityFacility(CityFacilityType.Stable, 1));
            }

            if (Tier >= CityTier.Fortress)
            {
                Facilities.Add(new CityFacility(CityFacilityType.Arsenal, 1));
            }

            if (Tier >= CityTier.Capital)
            {
                Facilities.Add(new CityFacility(CityFacilityType.Academy, 1));
            }
        }

        /// <summary>
        /// 獲取設施
        /// </summary>
        public CityFacility GetFacility(CityFacilityType type)
        {
            return Facilities.Find(f => f.Type == type);
        }

        /// <summary>
        /// 是否有指定設施
        /// </summary>
        public bool HasFacility(CityFacilityType type)
        {
            return Facilities.Exists(f => f.Type == type);
        }

        /// <summary>
        /// 獲取總防禦值（包含城牆加成）
        /// </summary>
        public int GetTotalDefense()
        {
            var wall = GetFacility(CityFacilityType.Wall);
            float wallBonus = wall?.GetBonus() ?? 0;
            return Mathf.RoundToInt(Defense * (1 + wallBonus));
        }

        /// <summary>
        /// 受到攻擊
        /// </summary>
        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - GetTotalDefense() / 10);
            Durability = Mathf.Max(0, Durability - actualDamage);

            if (Durability <= MaxDurability * 0.5f)
            {
                State = CityState.Damaged;
            }

            if (Durability <= 0)
            {
                // 城池陷落
                OnCityFall();
            }
        }

        /// <summary>
        /// 城池陷落處理
        /// </summary>
        private void OnCityFall()
        {
            GarrisonCount = 0;
            Durability = MaxDurability / 4; // 恢復部分耐久
            State = CityState.Normal;
        }

        /// <summary>
        /// 恢復耐久度
        /// </summary>
        public void Repair(int amount)
        {
            Durability = Mathf.Min(MaxDurability, Durability + amount);

            if (Durability > MaxDurability * 0.5f && State == CityState.Damaged)
            {
                State = CityState.Normal;
            }
        }
    }
}

