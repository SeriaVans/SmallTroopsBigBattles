using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;

namespace SmallTroopsBigBattles.Game.Army
{
    /// <summary>
    /// 軍隊管理器 - 負責士兵的招募與管理
    /// </summary>
    public class ArmyManager : SingletonBase<ArmyManager>
    {
        /// <summary>
        /// 當前玩家的軍隊數據
        /// </summary>
        public PlayerArmy PlayerArmy { get; private set; }

        protected override void OnSingletonAwake()
        {
            Debug.Log("[ArmyManager] 軍隊管理器初始化完成");
        }

        /// <summary>
        /// 初始化玩家軍隊
        /// </summary>
        public void Initialize(PlayerArmy army)
        {
            PlayerArmy = army;
            Debug.Log($"[ArmyManager] 載入玩家軍隊 - 總計 {army.TotalSoldiers} 名士兵");
        }

        /// <summary>
        /// 招募士兵
        /// </summary>
        public int RecruitSoldiers(SoldierType type, int count)
        {
            if (PlayerArmy == null) return 0;

            // 檢查資源
            var cost = GetRecruitCost(type, count);
            if (!Resource.ResourceManager.Instance.HasEnoughResources(cost.copper, 0, 0, cost.food))
            {
                Debug.LogWarning("[ArmyManager] 資源不足，無法招募");
                return 0;
            }

            // 招募
            int actualCount = PlayerArmy.RecruitSoldiers(type, count);
            if (actualCount > 0)
            {
                // 扣除資源
                var actualCost = GetRecruitCost(type, actualCount);
                Resource.ResourceManager.Instance.ConsumeResources(actualCost.copper, 0, 0, actualCost.food);
                Debug.Log($"[ArmyManager] 招募 {actualCount} 名 {GetSoldierDisplayName(type)}");
            }

            return actualCount;
        }

        /// <summary>
        /// 獲取士兵數量
        /// </summary>
        public int GetSoldierCount(SoldierType type)
        {
            return PlayerArmy?.GetSoldierCount(type) ?? 0;
        }

        /// <summary>
        /// 獲取總士兵數量
        /// </summary>
        public int GetTotalSoldiers()
        {
            return PlayerArmy?.TotalSoldiers ?? 0;
        }

        /// <summary>
        /// 獲取可招募空間
        /// </summary>
        public int GetAvailableSpace()
        {
            return PlayerArmy?.AvailableSpace ?? 0;
        }

        /// <summary>
        /// 獲取招募消耗
        /// </summary>
        public (int copper, int food) GetRecruitCost(SoldierType type, int count)
        {
            int costPerUnit = type switch
            {
                SoldierType.Spearman => 10,
                SoldierType.Shieldman => 15,
                SoldierType.Cavalry => 25,
                SoldierType.Archer => 20,
                _ => 10
            };

            return (costPerUnit * count, costPerUnit / 2 * count);
        }

        /// <summary>
        /// 獲取士兵顯示名稱
        /// </summary>
        public static string GetSoldierDisplayName(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => "槍兵",
                SoldierType.Shieldman => "盾兵",
                SoldierType.Cavalry => "騎兵",
                SoldierType.Archer => "弓兵",
                _ => "未知"
            };
        }

        /// <summary>
        /// 獲取士兵描述
        /// </summary>
        public static string GetSoldierDescription(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => "中等攻防，克制騎兵",
                SoldierType.Shieldman => "高防禦，抗弓兵傷害",
                SoldierType.Cavalry => "高機動，克制盾兵",
                SoldierType.Archer => "遠程攻擊，被所有兵種克制",
                _ => ""
            };
        }

        /// <summary>
        /// 獲取兵種克制關係
        /// </summary>
        public static float GetCounterMultiplier(SoldierType attacker, SoldierType defender)
        {
            // 槍兵克制騎兵
            if (attacker == SoldierType.Spearman && defender == SoldierType.Cavalry)
                return 1.5f;

            // 騎兵克制盾兵
            if (attacker == SoldierType.Cavalry && defender == SoldierType.Shieldman)
                return 1.5f;

            // 所有兵種克制弓兵
            if (defender == SoldierType.Archer)
                return 1.5f;

            // 盾兵對弓兵有抗性
            if (attacker == SoldierType.Archer && defender == SoldierType.Shieldman)
                return 0.5f;

            return 1f;
        }
    }
}

