using System;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Data
{
    /// <summary>
    /// 玩家資源數據（所有領地共用）
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        public long PlayerId;

        // 四種基礎資源
        private int _copper;
        private int _wood;
        private int _stone;
        private int _food;

        /// <summary>
        /// 銅錢 - 通用貨幣
        /// </summary>
        public int Copper
        {
            get => _copper;
            set => SetResource(ref _copper, value, ResourceType.Copper);
        }

        /// <summary>
        /// 木材 - 建築材料
        /// </summary>
        public int Wood
        {
            get => _wood;
            set => SetResource(ref _wood, value, ResourceType.Wood);
        }

        /// <summary>
        /// 石頭 - 建築材料
        /// </summary>
        public int Stone
        {
            get => _stone;
            set => SetResource(ref _stone, value, ResourceType.Stone);
        }

        /// <summary>
        /// 糧草 - 士兵消耗
        /// </summary>
        public int Food
        {
            get => _food;
            set => SetResource(ref _food, value, ResourceType.Food);
        }

        // 資源上限
        public int MaxCopper = 100000;
        public int MaxWood = 50000;
        public int MaxStone = 50000;
        public int MaxFood = 80000;

        public PlayerResources()
        {
            // 初始資源
            _copper = 1000;
            _wood = 500;
            _stone = 500;
            _food = 800;
        }

        /// <summary>
        /// 設置資源並觸發事件
        /// </summary>
        private void SetResource(ref int field, int value, ResourceType type)
        {
            int maxValue = type switch
            {
                ResourceType.Copper => MaxCopper,
                ResourceType.Wood => MaxWood,
                ResourceType.Stone => MaxStone,
                ResourceType.Food => MaxFood,
                _ => int.MaxValue
            };

            int oldValue = field;
            field = Math.Clamp(value, 0, maxValue);

            if (oldValue != field && EventManager.HasInstance)
            {
                EventManager.Instance.Publish(new ResourceChangedEvent(type, oldValue, field));
            }
        }

        /// <summary>
        /// 獲取指定類型的資源數量
        /// </summary>
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

        /// <summary>
        /// 設置指定類型的資源數量
        /// </summary>
        public void SetResource(ResourceType type, int value)
        {
            switch (type)
            {
                case ResourceType.Copper:
                    Copper = value;
                    break;
                case ResourceType.Wood:
                    Wood = value;
                    break;
                case ResourceType.Stone:
                    Stone = value;
                    break;
                case ResourceType.Food:
                    Food = value;
                    break;
            }
        }

        /// <summary>
        /// 增加資源
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            SetResource(type, GetResource(type) + amount);
        }

        /// <summary>
        /// 消耗資源
        /// </summary>
        /// <returns>是否成功消耗</returns>
        public bool ConsumeResource(ResourceType type, int amount)
        {
            if (GetResource(type) >= amount)
            {
                SetResource(type, GetResource(type) - amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 檢查是否有足夠的資源
        /// </summary>
        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }

        /// <summary>
        /// 檢查是否有足夠的多種資源
        /// </summary>
        public bool HasEnoughResources(int copper = 0, int wood = 0, int stone = 0, int food = 0)
        {
            return Copper >= copper && Wood >= wood && Stone >= stone && Food >= food;
        }

        /// <summary>
        /// 消耗多種資源
        /// </summary>
        /// <returns>是否成功消耗</returns>
        public bool ConsumeResources(int copper = 0, int wood = 0, int stone = 0, int food = 0)
        {
            if (!HasEnoughResources(copper, wood, stone, food))
                return false;

            Copper -= copper;
            Wood -= wood;
            Stone -= stone;
            Food -= food;
            return true;
        }
    }
}

