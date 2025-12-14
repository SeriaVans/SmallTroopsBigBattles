namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// 資源類型
    /// </summary>
    public enum ResourceType
    {
        Copper,     // 銅錢
        Wood,       // 木材
        Stone,      // 石材
        Food        // 糧草
    }

    /// <summary>
    /// 士兵類型
    /// </summary>
    public enum SoldierType
    {
        Spearman,   // 槍兵
        Shieldman,  // 盾兵
        Cavalry,    // 騎兵
        Archer      // 弓兵
    }

    /// <summary>
    /// 士兵狀態
    /// </summary>
    public enum SoldierState
    {
        Idle,
        Moving,
        Attacking,
        Retreating,
        Dead
    }

    /// <summary>
    /// 領地建築類型
    /// </summary>
    public enum TerritoryBuildingType
    {
        Farm,           // 農田
        Lumberyard,     // 伐木場
        Quarry,         // 採石場
        Barracks,       // 兵營
        Market,         // 市場
        Warehouse,      // 倉庫
        Academy         // 學院
    }

    /// <summary>
    /// 城市設施類型
    /// </summary>
    public enum CityFacilityType
    {
        Recruitment,    // 徵兵處
        Armory,         // 武器庫
        Stable,         // 馬廄
        Archery,        // 射箭場
        Wall,           // 城牆
        Tower,          // 箭塔
        Palace          // 宮殿
    }

    /// <summary>
    /// 地圖節點類型
    /// </summary>
    public enum NodeType
    {
        City,           // 城市
        Fortress,       // 要塞
        Village,        // 村莊
        Resource,       // 資源點
        Crossroad       // 路口
    }

    /// <summary>
    /// 地形類型
    /// </summary>
    public enum TerrainType
    {
        Plain,          // 平原
        Forest,         // 森林
        Mountain,       // 山地
        River,          // 河流
        Swamp           // 沼澤
    }

    /// <summary>
    /// 將領職業
    /// </summary>
    public enum GeneralClass
    {
        Commander,      // 統帥 (步兵加成)
        Vanguard,       // 先鋒 (騎兵加成)
        Strategist      // 軍師 (弓兵加成)
    }

    /// <summary>
    /// 戰鬥狀態
    /// </summary>
    public enum BattleState
    {
        Preparing,      // 準備中
        InProgress,     // 進行中
        Ended           // 已結束
    }

    /// <summary>
    /// 國家角色
    /// </summary>
    public enum FactionRole
    {
        King,           // 國王
        General,        // 將軍
        Officer,        // 軍官
        Soldier         // 士兵
    }

    /// <summary>
    /// 遊戲階段
    /// </summary>
    public enum GamePhase
    {
        Development,    // 發展期
        Conquest,       // 征服期
        Final,          // 決戰期
        Settlement      // 結算期
    }

    /// <summary>
    /// 科技類別
    /// </summary>
    public enum TechCategory
    {
        Military,       // 軍事
        Economy,        // 經濟
        Development     // 發展
    }

    /// <summary>
    /// 任務類型
    /// </summary>
    public enum QuestType
    {
        Daily,          // 每日任務
        Main,           // 主線任務
        Weekly,         // 週常任務
        Achievement     // 成就
    }

    /// <summary>
    /// 聊天頻道
    /// </summary>
    public enum ChatChannel
    {
        Nation          // 國家頻道
    }
}
