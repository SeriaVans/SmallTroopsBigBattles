using System;
using System.Collections.Generic;

namespace SmallTroopsBigBattles.Game.Data
{
    /// <summary>
    /// 玩家基本資料
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public long PlayerId;
        public string Name;
        public int NationId;
        public int Level;
        public DateTime JoinTime;

        public PlayerData()
        {
            JoinTime = DateTime.Now;
            Level = 1;
        }

        public PlayerData(long playerId, string name, int nationId) : this()
        {
            PlayerId = playerId;
            Name = name;
            NationId = nationId;
        }
    }

    /// <summary>
    /// 玩家完整數據（包含所有子系統數據）
    /// </summary>
    [Serializable]
    public class PlayerFullData
    {
        public PlayerData BasicInfo;
        public PlayerResources Resources;
        public PlayerArmy Army;
        public PlayerTerritories Territories;
        public List<GeneralData> Generals;

        public PlayerFullData()
        {
            BasicInfo = new PlayerData();
            Resources = new PlayerResources();
            Army = new PlayerArmy();
            Territories = new PlayerTerritories();
            Generals = new List<GeneralData>();
        }

        public PlayerFullData(long playerId, string name, int nationId) : this()
        {
            BasicInfo = new PlayerData(playerId, name, nationId);
            Resources.PlayerId = playerId;
            Army.PlayerId = playerId;
            Territories.PlayerId = playerId;
        }
    }
}

