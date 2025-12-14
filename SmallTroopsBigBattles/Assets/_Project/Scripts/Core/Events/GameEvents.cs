namespace SmallTroopsBigBattles.Core.Events
{
    /// <summary>
    /// 遊戲事件基類
    /// </summary>
    public abstract class GameEvent
    {
    }

    #region 資源相關事件

    /// <summary>
    /// 資源變更事件
    /// </summary>
    public class ResourceChangedEvent : GameEvent
    {
        public ResourceType ResourceType { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public int Delta => NewValue - OldValue;

        public ResourceChangedEvent(ResourceType type, int oldValue, int newValue)
        {
            ResourceType = type;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// 資源類型
    /// </summary>
    public enum ResourceType
    {
        Copper,     // 銅錢
        Wood,       // 木材
        Stone,      // 石頭
        Food        // 糧草
    }

    #endregion

    #region 領地相關事件

    /// <summary>
    /// 領地選擇變更事件
    /// </summary>
    public class TerritorySelectedEvent : GameEvent
    {
        public int TerritoryId { get; }

        public TerritorySelectedEvent(int territoryId)
        {
            TerritoryId = territoryId;
        }
    }

    /// <summary>
    /// 建築建造完成事件
    /// </summary>
    public class BuildingConstructedEvent : GameEvent
    {
        public int TerritoryId { get; }
        public int SlotIndex { get; }
        public BuildingType BuildingType { get; }

        public BuildingConstructedEvent(int territoryId, int slotIndex, BuildingType buildingType)
        {
            TerritoryId = territoryId;
            SlotIndex = slotIndex;
            BuildingType = buildingType;
        }
    }

    /// <summary>
    /// 建築升級完成事件
    /// </summary>
    public class BuildingUpgradedEvent : GameEvent
    {
        public int TerritoryId { get; }
        public int SlotIndex { get; }
        public int NewLevel { get; }

        public BuildingUpgradedEvent(int territoryId, int slotIndex, int newLevel)
        {
            TerritoryId = territoryId;
            SlotIndex = slotIndex;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// 建築類型
    /// </summary>
    public enum BuildingType
    {
        None,           // 空
        Mansion,        // 府邸 - 領地核心
        Farm,           // 農場 - 產糧草
        LumberMill,     // 伐木場 - 產木材
        Quarry,         // 採石場 - 產石頭
        Mint,           // 鑄幣坊 - 產銅錢
        Barracks,       // 兵營 - 訓練步兵
        Stable,         // 馬廄 - 訓練騎兵
        ArcheryRange,   // 射擊場 - 訓練弓兵
        Academy,        // 學院 - 科技研究
        Hospital        // 醫館 - 傷兵恢復
    }

    #endregion

    #region 軍隊相關事件

    /// <summary>
    /// 士兵數量變更事件
    /// </summary>
    public class SoldiersChangedEvent : GameEvent
    {
        public SoldierType SoldierType { get; }
        public int OldCount { get; }
        public int NewCount { get; }

        public SoldiersChangedEvent(SoldierType soldierType, int oldCount, int newCount)
        {
            SoldierType = soldierType;
            OldCount = oldCount;
            NewCount = newCount;
        }
    }

    /// <summary>
    /// 士兵訓練完成事件
    /// </summary>
    public class SoldierTrainedEvent : GameEvent
    {
        public SoldierType SoldierType { get; }
        public int Count { get; }

        public SoldierTrainedEvent(SoldierType soldierType, int count)
        {
            SoldierType = soldierType;
            Count = count;
        }
    }

    /// <summary>
    /// 士兵類型
    /// </summary>
    public enum SoldierType
    {
        Spearman,   // 槍兵 - 克制騎兵
        Shieldman,  // 盾兵 - 抗弓兵傷害
        Cavalry,    // 騎兵 - 克制盾兵
        Archer      // 弓兵 - 遠程，被所有人克制
    }

    #endregion

    #region 將領相關事件

    /// <summary>
    /// 將領獲得事件
    /// </summary>
    public class GeneralObtainedEvent : GameEvent
    {
        public long GeneralId { get; }

        public GeneralObtainedEvent(long generalId)
        {
            GeneralId = generalId;
        }
    }

    /// <summary>
    /// 將領升級事件
    /// </summary>
    public class GeneralLevelUpEvent : GameEvent
    {
        public long GeneralId { get; }
        public int NewLevel { get; }

        public GeneralLevelUpEvent(long generalId, int newLevel)
        {
            GeneralId = generalId;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// 將領職業
    /// </summary>
    public enum GeneralClass
    {
        Commander,      // 統帥 - 步兵加成
        Vanguard,       // 先鋒 - 騎兵加成
        Strategist      // 軍師 - 弓兵加成
    }

    #endregion

    #region UI 相關事件

    /// <summary>
    /// 面板開啟事件
    /// </summary>
    public class PanelOpenedEvent : GameEvent
    {
        public string PanelName { get; }

        public PanelOpenedEvent(string panelName)
        {
            PanelName = panelName;
        }
    }

    /// <summary>
    /// 面板關閉事件
    /// </summary>
    public class PanelClosedEvent : GameEvent
    {
        public string PanelName { get; }

        public PanelClosedEvent(string panelName)
        {
            PanelName = panelName;
        }
    }

    #endregion

    #region 遊戲狀態事件

    /// <summary>
    /// 遊戲初始化完成事件
    /// </summary>
    public class GameInitializedEvent : GameEvent
    {
    }

    /// <summary>
    /// 玩家登入事件
    /// </summary>
    public class PlayerLoggedInEvent : GameEvent
    {
        public long PlayerId { get; }
        public string PlayerName { get; }

        public PlayerLoggedInEvent(long playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }

    #endregion
}

