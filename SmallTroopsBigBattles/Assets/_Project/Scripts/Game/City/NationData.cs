using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.City
{
    /// <summary>
    /// 國家陣營
    /// </summary>
    public enum NationFaction
    {
        Wei,    // 魏
        Shu,    // 蜀
        Wu      // 吳
    }

    /// <summary>
    /// 國家狀態
    /// </summary>
    public enum NationState
    {
        Active,     // 活躍
        Weakened,   // 衰弱
        Defeated    // 戰敗
    }

    /// <summary>
    /// 國家數據
    /// </summary>
    [Serializable]
    public class NationData
    {
        /// <summary>國家唯一 ID</summary>
        public string NationId;

        /// <summary>國家名稱</summary>
        public string NationName;

        /// <summary>國家陣營</summary>
        public NationFaction Faction;

        /// <summary>國家狀態</summary>
        public NationState State;

        /// <summary>首都城池 ID</summary>
        public string CapitalCityId;

        /// <summary>國家顏色（UI 顯示用）</summary>
        public Color NationColor;

        /// <summary>控制的城池 ID 列表</summary>
        public List<string> ControlledCityIds = new List<string>();

        /// <summary>控制的地圖節點 ID 列表</summary>
        public List<string> ControlledNodeIds = new List<string>();

        /// <summary>所有成員玩家 ID</summary>
        public List<string> MemberPlayerIds = new List<string>();

        /// <summary>國家科技等級</summary>
        public NationTech Technology;

        /// <summary>國家國庫資源</summary>
        public NationTreasury Treasury;

        /// <summary>國家總兵力</summary>
        public int TotalMilitaryPower;

        /// <summary>國家聲望值</summary>
        public int Prestige;

        /// <summary>
        /// 建構函式
        /// </summary>
        public NationData()
        {
            NationId = Guid.NewGuid().ToString();
            Technology = new NationTech();
            Treasury = new NationTreasury();
            ControlledCityIds = new List<string>();
            ControlledNodeIds = new List<string>();
            MemberPlayerIds = new List<string>();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public NationData(string name, NationFaction faction)
        {
            NationId = $"nation_{faction.ToString().ToLower()}";
            NationName = name;
            Faction = faction;
            State = NationState.Active;
            Technology = new NationTech();
            Treasury = new NationTreasury();
            ControlledCityIds = new List<string>();
            ControlledNodeIds = new List<string>();
            MemberPlayerIds = new List<string>();

            // 設置國家顏色
            NationColor = faction switch
            {
                NationFaction.Wei => new Color(0.2f, 0.4f, 0.8f), // 藍色
                NationFaction.Shu => new Color(0.2f, 0.7f, 0.3f), // 綠色
                NationFaction.Wu => new Color(0.8f, 0.2f, 0.2f), // 紅色
                _ => Color.gray
            };
        }

        /// <summary>
        /// 添加城池控制
        /// </summary>
        public void AddCity(string cityId)
        {
            if (!ControlledCityIds.Contains(cityId))
            {
                ControlledCityIds.Add(cityId);
            }
        }

        /// <summary>
        /// 移除城池控制
        /// </summary>
        public void RemoveCity(string cityId)
        {
            ControlledCityIds.Remove(cityId);

            // 如果失去首都，選擇新首都
            if (cityId == CapitalCityId && ControlledCityIds.Count > 0)
            {
                CapitalCityId = ControlledCityIds[0];
            }
        }

        /// <summary>
        /// 添加節點控制
        /// </summary>
        public void AddNode(string nodeId)
        {
            if (!ControlledNodeIds.Contains(nodeId))
            {
                ControlledNodeIds.Add(nodeId);
            }
        }

        /// <summary>
        /// 移除節點控制
        /// </summary>
        public void RemoveNode(string nodeId)
        {
            ControlledNodeIds.Remove(nodeId);
        }

        /// <summary>
        /// 添加成員玩家
        /// </summary>
        public void AddMember(string playerId)
        {
            if (!MemberPlayerIds.Contains(playerId))
            {
                MemberPlayerIds.Add(playerId);
            }
        }

        /// <summary>
        /// 移除成員玩家
        /// </summary>
        public void RemoveMember(string playerId)
        {
            MemberPlayerIds.Remove(playerId);
        }

        /// <summary>
        /// 獲取國家實力評估
        /// </summary>
        public int GetPowerRating()
        {
            int rating = 0;
            rating += ControlledCityIds.Count * 1000;
            rating += ControlledNodeIds.Count * 100;
            rating += MemberPlayerIds.Count * 500;
            rating += TotalMilitaryPower / 10;
            rating += Prestige;
            return rating;
        }

        /// <summary>
        /// 檢查國家是否戰敗
        /// </summary>
        public bool CheckDefeat()
        {
            // 失去首都且無城池
            if (ControlledCityIds.Count == 0)
            {
                State = NationState.Defeated;
                return true;
            }

            // 衰弱狀態
            if (ControlledCityIds.Count <= 1)
            {
                State = NationState.Weakened;
            }

            return false;
        }
    }

    /// <summary>
    /// 國家科技
    /// </summary>
    [Serializable]
    public class NationTech
    {
        /// <summary>軍事科技等級</summary>
        public int MilitaryLevel;

        /// <summary>經濟科技等級</summary>
        public int EconomyLevel;

        /// <summary>防禦科技等級</summary>
        public int DefenseLevel;

        /// <summary>外交科技等級</summary>
        public int DiplomacyLevel;

        /// <summary>
        /// 獲取攻擊加成
        /// </summary>
        public float GetAttackBonus() => MilitaryLevel * 0.05f;

        /// <summary>
        /// 獲取資源產出加成
        /// </summary>
        public float GetResourceBonus() => EconomyLevel * 0.05f;

        /// <summary>
        /// 獲取防禦加成
        /// </summary>
        public float GetDefenseBonus() => DefenseLevel * 0.05f;
    }

    /// <summary>
    /// 國家國庫
    /// </summary>
    [Serializable]
    public class NationTreasury
    {
        /// <summary>銅錢</summary>
        public long Copper;

        /// <summary>木材</summary>
        public long Wood;

        /// <summary>石頭</summary>
        public long Stone;

        /// <summary>糧草</summary>
        public long Food;

        /// <summary>戰爭基金（用於國戰）</summary>
        public long WarFund;

        /// <summary>
        /// 捐獻資源
        /// </summary>
        public void Donate(int copper, int wood, int stone, int food)
        {
            Copper += copper;
            Wood += wood;
            Stone += stone;
            Food += food;
        }

        /// <summary>
        /// 消耗資源
        /// </summary>
        public bool Consume(int copper, int wood, int stone, int food)
        {
            if (Copper < copper || Wood < wood || Stone < stone || Food < food)
                return false;

            Copper -= copper;
            Wood -= wood;
            Stone -= stone;
            Food -= food;
            return true;
        }
    }
}

