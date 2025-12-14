using System;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Data
{
    /// <summary>
    /// 玩家軍隊數據
    /// </summary>
    [Serializable]
    public class PlayerArmy
    {
        public long PlayerId;

        /// <summary>
        /// 士兵總上限
        /// </summary>
        public const int MaxSoldiers = 5000;

        // 各兵種數量
        private int _spearman;
        private int _shieldman;
        private int _cavalry;
        private int _archer;

        /// <summary>
        /// 槍兵數量 - 克制騎兵
        /// </summary>
        public int Spearman
        {
            get => _spearman;
            set => SetSoldierCount(ref _spearman, value, SoldierType.Spearman);
        }

        /// <summary>
        /// 盾兵數量 - 抗弓兵傷害
        /// </summary>
        public int Shieldman
        {
            get => _shieldman;
            set => SetSoldierCount(ref _shieldman, value, SoldierType.Shieldman);
        }

        /// <summary>
        /// 騎兵數量 - 克制盾兵
        /// </summary>
        public int Cavalry
        {
            get => _cavalry;
            set => SetSoldierCount(ref _cavalry, value, SoldierType.Cavalry);
        }

        /// <summary>
        /// 弓兵數量 - 遠程攻擊
        /// </summary>
        public int Archer
        {
            get => _archer;
            set => SetSoldierCount(ref _archer, value, SoldierType.Archer);
        }

        /// <summary>
        /// 當前士兵總數
        /// </summary>
        public int TotalSoldiers => Spearman + Shieldman + Cavalry + Archer;

        /// <summary>
        /// 可招募的空間
        /// </summary>
        public int AvailableSpace => MaxSoldiers - TotalSoldiers;

        /// <summary>
        /// 設置士兵數量並觸發事件
        /// </summary>
        private void SetSoldierCount(ref int field, int value, SoldierType type)
        {
            int oldValue = field;
            field = Math.Max(0, value);

            if (oldValue != field && EventManager.HasInstance)
            {
                EventManager.Instance.Publish(new SoldiersChangedEvent(type, oldValue, field));
            }
        }

        /// <summary>
        /// 獲取指定兵種的數量
        /// </summary>
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

        /// <summary>
        /// 設置指定兵種的數量
        /// </summary>
        public void SetSoldierCount(SoldierType type, int count)
        {
            switch (type)
            {
                case SoldierType.Spearman:
                    Spearman = count;
                    break;
                case SoldierType.Shieldman:
                    Shieldman = count;
                    break;
                case SoldierType.Cavalry:
                    Cavalry = count;
                    break;
                case SoldierType.Archer:
                    Archer = count;
                    break;
            }
        }

        /// <summary>
        /// 招募士兵
        /// </summary>
        /// <returns>實際招募的數量</returns>
        public int RecruitSoldiers(SoldierType type, int count)
        {
            int actualCount = Math.Min(count, AvailableSpace);
            if (actualCount <= 0) return 0;

            SetSoldierCount(type, GetSoldierCount(type) + actualCount);

            if (EventManager.HasInstance)
            {
                EventManager.Instance.Publish(new SoldierTrainedEvent(type, actualCount));
            }

            return actualCount;
        }

        /// <summary>
        /// 損失士兵
        /// </summary>
        public void LoseSoldiers(SoldierType type, int count)
        {
            int current = GetSoldierCount(type);
            SetSoldierCount(type, Math.Max(0, current - count));
        }

        /// <summary>
        /// 檢查是否有足夠的士兵
        /// </summary>
        public bool HasEnoughSoldiers(SoldierType type, int count)
        {
            return GetSoldierCount(type) >= count;
        }
    }
}

