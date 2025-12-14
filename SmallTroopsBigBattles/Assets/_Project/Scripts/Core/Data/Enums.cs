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
        Spearman,   // 槍兵 - 克制騎兵
        Shieldman,  // 盾兵 - 抗弓兵傷害
        Cavalry,    // 騎兵 - 克制盾兵
        Archer      // 弓兵 - 遠程，被所有人克制
    }

    /// <summary>
    /// 士兵狀態
    /// </summary>
    public enum SoldierState
    {
        Advancing,  // 前進中 (初始狀態)
        Moving,     // 移動中 (追擊目標)
        Attacking,  // 攻擊中
        Retreating, // 撤退中
        Dead        // 死亡
    }

    /// <summary>
    /// 領地建築類型
    /// </summary>
    public enum TerritoryBuildingType
    {
        // 核心建築
        Mansion,        // 府邸 - 領地核心 (預設，不可拆除)

        // 資源類
        Farm,           // 農場 - 產糧草
        LumberMill,     // 伐木場 - 產木材
        Quarry,         // 採石場 - 產石頭
        Mint,           // 鑄幣坊 - 產銅錢

        // 軍事類
        Barracks,       // 兵營 - 訓練步兵 (槍兵、盾兵)
        Stable,         // 馬廄 - 訓練騎兵
        ArcheryRange,   // 射擊場 - 訓練弓兵

        // 功能類
        Academy,        // 學院 - 科技研究
        Hospital        // 醫館 - 傷兵恢復
    }

    /// <summary>
    /// 城池設施類型
    /// </summary>
    public enum CityFacilityType
    {
        Wall,           // 城牆
        Market,         // 市集
        Farm,           // 農莊
        LumberMill,     // 木工坊
        Quarry          // 石料場
    }

    /// <summary>
    /// 地圖節點類型
    /// </summary>
    public enum NodeType
    {
        Capital,    // 主城
        City,       // 城池
        Pass,       // 關隘
        Resource,   // 資源點
        Throne      // 王城
    }

    /// <summary>
    /// 地形類型
    /// </summary>
    public enum TerrainType
    {
        Plain,      // 平原 - 速度正常
        Mountain,   // 山地 - 速度減慢
        Forest,     // 森林 - 速度稍慢，有伏擊加成
        River,      // 河流 - 需要渡河時間
        Pass        // 關隘 - 防守方加成
    }

    /// <summary>
    /// 將領職業
    /// </summary>
    public enum GeneralClass
    {
        Commander,      // 統帥 - 步兵加成 (槍兵、盾兵)
        Vanguard,       // 先鋒 - 騎兵加成
        Strategist      // 軍師 - 弓兵加成
    }

    /// <summary>
    /// 戰場狀態
    /// </summary>
    public enum BattleState
    {
        Preparing,  // 準備中
        Fighting,   // 戰鬥中
        Settling,   // 結算中
        Ended       // 已結束
    }

    /// <summary>
    /// 戰場陣營角色
    /// </summary>
    public enum FactionRole
    {
        Attacker,   // 攻方
        Defender,   // 守方
        Third       // 第三方
    }

    /// <summary>
    /// 劇本階段
    /// </summary>
    public enum GamePhase
    {
        Development,    // 發展期
        Conquest,       // 爭霸期
        Final,          // 決戰期
        Settlement      // 結算期
    }

    /// <summary>
    /// 科技類別
    /// </summary>
    public enum TechCategory
    {
        Military,   // 軍事
        Economy,    // 經濟
        Development // 發展
    }

    /// <summary>
    /// 任務類型
    /// </summary>
    public enum QuestType
    {
        Main,           // 主線任務
        Daily,          // 每日任務
        Achievement     // 成就
    }

    /// <summary>
    /// 聊天頻道
    /// </summary>
    public enum ChatChannel
    {
        World,      // 世界頻道
        Nation,     // 國家頻道
        Private     // 私聊
    }
}
