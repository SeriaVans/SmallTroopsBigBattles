using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;

namespace SmallTroopsBigBattles.Game.Territory
{
    /// <summary>
    /// 領地管理器 - 負責玩家領地的管理
    /// </summary>
    public class TerritoryManager : SingletonBase<TerritoryManager>
    {
        /// <summary>
        /// 當前玩家的領地數據
        /// </summary>
        public PlayerTerritories PlayerTerritories { get; private set; }

        /// <summary>
        /// 當前選中的領地
        /// </summary>
        public TerritoryData CurrentTerritory { get; private set; }

        /// <summary>
        /// 當前選中的領地索引
        /// </summary>
        public int CurrentTerritoryIndex { get; private set; } = 0;

        protected override void OnSingletonAwake()
        {
            Debug.Log("[TerritoryManager] 領地管理器初始化完成");
        }

        /// <summary>
        /// 初始化玩家領地
        /// </summary>
        public void Initialize(PlayerTerritories territories)
        {
            PlayerTerritories = territories;

            // 如果沒有領地，創建第一個
            if (territories.TerritoryCount == 0)
            {
                CreateFirstTerritory();
            }

            // 選擇第一個領地
            SelectTerritory(0);

            Debug.Log($"[TerritoryManager] 載入玩家領地 - 共 {territories.TerritoryCount} 個領地");
        }

        /// <summary>
        /// 創建第一個領地（新玩家）
        /// </summary>
        private void CreateFirstTerritory()
        {
            var territory = PlayerTerritories.CreateTerritory(1);  // 預設城池 ID = 1
            Debug.Log("[TerritoryManager] 為新玩家創建第一個領地");
        }

        /// <summary>
        /// 選擇領地
        /// </summary>
        public void SelectTerritory(int index)
        {
            if (PlayerTerritories == null || index < 0 || index >= PlayerTerritories.TerritoryCount)
            {
                return;
            }

            CurrentTerritoryIndex = index;
            CurrentTerritory = PlayerTerritories.Territories[index];

            EventManager.Instance.Publish(new TerritorySelectedEvent(CurrentTerritory.TerritoryId));
            Debug.Log($"[TerritoryManager] 選擇領地 {CurrentTerritory.TerritoryId}");
        }

        /// <summary>
        /// 在當前領地建造建築
        /// </summary>
        public bool BuildBuilding(int slotIndex, BuildingType type)
        {
            if (CurrentTerritory == null) return false;

            // 檢查資源
            var cost = GetBuildingCost(type, 1);
            if (!Resource.ResourceManager.Instance.HasEnoughResources(cost.copper, cost.wood, cost.stone, cost.food))
            {
                Debug.LogWarning("[TerritoryManager] 資源不足，無法建造");
                return false;
            }

            // 建造
            var building = CurrentTerritory.BuildAt(slotIndex, type);
            if (building == null)
            {
                Debug.LogWarning("[TerritoryManager] 建造失敗，格位無效或已被佔用");
                return false;
            }

            // 扣除資源
            Resource.ResourceManager.Instance.ConsumeResources(cost.copper, cost.wood, cost.stone, cost.food);

            // 發送事件
            EventManager.Instance.Publish(new BuildingConstructedEvent(
                CurrentTerritory.TerritoryId, slotIndex, type));

            Debug.Log($"[TerritoryManager] 建造 {GetBuildingDisplayName(type)} 於格位 {slotIndex}");
            return true;
        }

        /// <summary>
        /// 升級建築
        /// </summary>
        public bool UpgradeBuilding(int slotIndex)
        {
            if (CurrentTerritory == null) return false;

            var building = CurrentTerritory.GetBuildingAt(slotIndex);
            if (building == null || !building.CanUpgrade)
            {
                Debug.LogWarning("[TerritoryManager] 升級失敗，建築不存在或無法升級");
                return false;
            }

            // 檢查資源
            var cost = GetBuildingCost(building.Type, building.Level + 1);
            if (!Resource.ResourceManager.Instance.HasEnoughResources(cost.copper, cost.wood, cost.stone, cost.food))
            {
                Debug.LogWarning("[TerritoryManager] 資源不足，無法升級");
                return false;
            }

            // 升級
            building.Upgrade();

            // 扣除資源
            Resource.ResourceManager.Instance.ConsumeResources(cost.copper, cost.wood, cost.stone, cost.food);

            // 發送事件
            EventManager.Instance.Publish(new BuildingUpgradedEvent(
                CurrentTerritory.TerritoryId, slotIndex, building.Level));

            Debug.Log($"[TerritoryManager] 升級 {GetBuildingDisplayName(building.Type)} 至 Lv{building.Level}");
            return true;
        }

        /// <summary>
        /// 拆除建築
        /// </summary>
        public bool DemolishBuilding(int slotIndex)
        {
            if (CurrentTerritory == null) return false;

            if (CurrentTerritory.DemolishAt(slotIndex))
            {
                Debug.Log($"[TerritoryManager] 拆除格位 {slotIndex} 的建築");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 獲取建築消耗
        /// </summary>
        public (int copper, int wood, int stone, int food) GetBuildingCost(BuildingType type, int level)
        {
            // 基礎消耗
            int baseCost = type switch
            {
                BuildingType.Farm => 100,
                BuildingType.LumberMill => 100,
                BuildingType.Quarry => 100,
                BuildingType.Mint => 150,
                BuildingType.Barracks => 200,
                BuildingType.Stable => 250,
                BuildingType.ArcheryRange => 200,
                BuildingType.Academy => 300,
                BuildingType.Hospital => 250,
                _ => 100
            };

            // 等級係數
            float levelMultiplier = 1f + (level - 1) * 0.5f;
            int cost = (int)(baseCost * levelMultiplier);

            return (cost, cost / 2, cost / 2, cost / 4);
        }

        /// <summary>
        /// 獲取建築的顯示名稱
        /// </summary>
        public static string GetBuildingDisplayName(BuildingType type)
        {
            return type switch
            {
                BuildingType.None => "空地",
                BuildingType.Mansion => "府邸",
                BuildingType.Farm => "農場",
                BuildingType.LumberMill => "伐木場",
                BuildingType.Quarry => "採石場",
                BuildingType.Mint => "鑄幣坊",
                BuildingType.Barracks => "兵營",
                BuildingType.Stable => "馬廄",
                BuildingType.ArcheryRange => "射擊場",
                BuildingType.Academy => "學院",
                BuildingType.Hospital => "醫館",
                _ => "未知"
            };
        }

        /// <summary>
        /// 獲取建築的描述
        /// </summary>
        public static string GetBuildingDescription(BuildingType type)
        {
            return type switch
            {
                BuildingType.Mansion => "領地核心建築，決定領地等級",
                BuildingType.Farm => "產出糧草，供養士兵",
                BuildingType.LumberMill => "產出木材，用於建設",
                BuildingType.Quarry => "產出石頭，用於建設",
                BuildingType.Mint => "產出銅錢，通用貨幣",
                BuildingType.Barracks => "訓練步兵（槍兵、盾兵）",
                BuildingType.Stable => "訓練騎兵",
                BuildingType.ArcheryRange => "訓練弓兵",
                BuildingType.Academy => "研究科技",
                BuildingType.Hospital => "治療傷兵",
                _ => ""
            };
        }
    }
}

