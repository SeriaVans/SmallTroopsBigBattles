using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Config
{
    /// <summary>
    /// 士兵配置
    /// </summary>
    [CreateAssetMenu(fileName = "SoldierConfig", menuName = "SmallTroopsBigBattles/Config/Soldier Config")]
    public class SoldierConfig : ScriptableObject
    {
        public List<SoldierConfigData> Soldiers = new List<SoldierConfigData>();

        private Dictionary<SoldierType, SoldierConfigData> _soldierDict;

        public SoldierConfigData GetConfig(SoldierType type)
        {
            if (_soldierDict == null)
            {
                _soldierDict = new Dictionary<SoldierType, SoldierConfigData>();
                foreach (var soldier in Soldiers)
                {
                    _soldierDict[soldier.Type] = soldier;
                }
            }

            return _soldierDict.TryGetValue(type, out var config) ? config : null;
        }

        private static SoldierConfig _instance;
        public static SoldierConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SoldierConfig>("Config/SoldierConfig");
                }
                return _instance;
            }
        }
    }

    [Serializable]
    public class SoldierConfigData
    {
        public SoldierType Type;
        public string Name;
        public string Description;

        [Header("基礎屬性")]
        public int BaseHp = 100;
        public int BaseAttack = 10;
        public int BaseDefense = 5;
        public float MoveSpeed = 2f;
        public float AttackRange = 1f;
        public float AttackSpeed = 1f;  // 攻擊間隔秒數

        [Header("訓練成本")]
        public int CopperCost;
        public int WoodCost;
        public int StoneCost;
        public int FoodCost;

        [Header("訓練時間")]
        public int TrainingTime = 10;  // 秒/人

        [Header("克制關係")]
        public SoldierType[] StrongAgainst;   // 克制的兵種
        public SoldierType[] WeakAgainst;     // 被克制的兵種
        public float CounterDamageBonus = 0.3f;  // 克制傷害加成

        /// <summary>
        /// 取得訓練成本
        /// </summary>
        public Dictionary<ResourceType, int> GetTrainingCost(int count)
        {
            var costs = new Dictionary<ResourceType, int>();

            if (CopperCost > 0)
                costs[ResourceType.Copper] = CopperCost * count;
            if (WoodCost > 0)
                costs[ResourceType.Wood] = WoodCost * count;
            if (StoneCost > 0)
                costs[ResourceType.Stone] = StoneCost * count;
            if (FoodCost > 0)
                costs[ResourceType.Food] = FoodCost * count;

            return costs;
        }

        /// <summary>
        /// 計算對目標的傷害倍率
        /// </summary>
        public float GetDamageMultiplier(SoldierType targetType)
        {
            // 克制關係
            if (StrongAgainst != null)
            {
                foreach (var type in StrongAgainst)
                {
                    if (type == targetType)
                        return 1 + CounterDamageBonus;
                }
            }

            // 被克制關係
            if (WeakAgainst != null)
            {
                foreach (var type in WeakAgainst)
                {
                    if (type == targetType)
                        return 1 - CounterDamageBonus;
                }
            }

            return 1f;
        }
    }
}
