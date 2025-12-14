using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.City
{
    /// <summary>
    /// 國家管理器 - 負責管理所有國家和城池
    /// </summary>
    public class NationManager : SingletonBase<NationManager>
    {
        /// <summary>所有國家</summary>
        private Dictionary<string, NationData> _nations = new Dictionary<string, NationData>();

        /// <summary>所有城池</summary>
        private Dictionary<string, CityData> _cities = new Dictionary<string, CityData>();

        /// <summary>當前玩家所屬國家 ID</summary>
        private string _playerNationId;

        /// <summary>國家列表（唯讀）</summary>
        public IReadOnlyDictionary<string, NationData> Nations => _nations;

        /// <summary>城池列表（唯讀）</summary>
        public IReadOnlyDictionary<string, CityData> Cities => _cities;

        /// <summary>當前玩家所屬國家</summary>
        public NationData PlayerNation =>
            !string.IsNullOrEmpty(_playerNationId) && _nations.TryGetValue(_playerNationId, out var nation)
                ? nation
                : null;

        /// <summary>
        /// 初始化國家管理器
        /// </summary>
        public void Initialize()
        {
            _nations.Clear();
            _cities.Clear();

            // 創建三國
            CreateInitialNations();

            // 創建初始城池
            CreateInitialCities();

            Debug.Log($"[NationManager] 初始化完成，國家數：{_nations.Count}，城池數：{_cities.Count}");
        }

        /// <summary>
        /// 創建初始國家
        /// </summary>
        private void CreateInitialNations()
        {
            // 創建魏國
            var wei = new NationData("魏", NationFaction.Wei);
            wei.Prestige = 1000;
            _nations[wei.NationId] = wei;

            // 創建蜀國
            var shu = new NationData("蜀", NationFaction.Shu);
            shu.Prestige = 800;
            _nations[shu.NationId] = shu;

            // 創建吳國
            var wu = new NationData("吳", NationFaction.Wu);
            wu.Prestige = 900;
            _nations[wu.NationId] = wu;
        }

        /// <summary>
        /// 創建初始城池
        /// </summary>
        private void CreateInitialCities()
        {
            // 蜀國首都
            var shuCapital = new CityData("成都", CityTier.Capital);
            shuCapital.NationId = "nation_shu";
            _cities[shuCapital.CityId] = shuCapital;
            GetNation("nation_shu")?.AddCity(shuCapital.CityId);
            GetNation("nation_shu").CapitalCityId = shuCapital.CityId;

            // 魏國首都
            var weiCapital = new CityData("洛陽", CityTier.Capital);
            weiCapital.NationId = "nation_wei";
            _cities[weiCapital.CityId] = weiCapital;
            GetNation("nation_wei")?.AddCity(weiCapital.CityId);
            GetNation("nation_wei").CapitalCityId = weiCapital.CityId;

            // 吳國首都
            var wuCapital = new CityData("建業", CityTier.Capital);
            wuCapital.NationId = "nation_wu";
            _cities[wuCapital.CityId] = wuCapital;
            GetNation("nation_wu")?.AddCity(wuCapital.CityId);
            GetNation("nation_wu").CapitalCityId = wuCapital.CityId;

            // 創建一些堡壘和城市
            CreateCity("虎牢關", CityTier.Fortress, null);  // 中立
            CreateCity("劍閣", CityTier.Fortress, null);    // 中立
            CreateCity("漢中", CityTier.City, "nation_shu");
            CreateCity("許昌", CityTier.City, "nation_wei");
            CreateCity("柴桑", CityTier.City, "nation_wu");
        }

        /// <summary>
        /// 創建城池
        /// </summary>
        private CityData CreateCity(string name, CityTier tier, string nationId)
        {
            var city = new CityData(name, tier);
            city.NationId = nationId;
            _cities[city.CityId] = city;

            if (!string.IsNullOrEmpty(nationId))
            {
                GetNation(nationId)?.AddCity(city.CityId);
            }

            return city;
        }

        /// <summary>
        /// 獲取國家
        /// </summary>
        public NationData GetNation(string nationId)
        {
            return _nations.TryGetValue(nationId, out var nation) ? nation : null;
        }

        /// <summary>
        /// 根據陣營獲取國家
        /// </summary>
        public NationData GetNationByFaction(NationFaction faction)
        {
            return _nations.Values.FirstOrDefault(n => n.Faction == faction);
        }

        /// <summary>
        /// 獲取城池
        /// </summary>
        public CityData GetCity(string cityId)
        {
            return _cities.TryGetValue(cityId, out var city) ? city : null;
        }

        /// <summary>
        /// 獲取國家的所有城池
        /// </summary>
        public List<CityData> GetCitiesByNation(string nationId)
        {
            return _cities.Values
                .Where(c => c.NationId == nationId)
                .ToList();
        }

        /// <summary>
        /// 加入國家
        /// </summary>
        public bool JoinNation(string playerId, string nationId)
        {
            var nation = GetNation(nationId);
            if (nation == null || nation.State == NationState.Defeated)
            {
                Debug.LogWarning($"[NationManager] 無法加入國家 {nationId}");
                return false;
            }

            // 離開原來的國家
            if (!string.IsNullOrEmpty(_playerNationId))
            {
                GetNation(_playerNationId)?.RemoveMember(playerId);
            }

            // 加入新國家
            nation.AddMember(playerId);
            _playerNationId = nationId;

            EventManager.Instance?.Publish(new PlayerJoinedNationEvent
            {
                PlayerId = playerId,
                NationId = nationId
            });

            Debug.Log($"[NationManager] 玩家 {playerId} 加入了 {nation.NationName}");
            return true;
        }

        /// <summary>
        /// 設置玩家所屬國家
        /// </summary>
        public void SetPlayerNation(string nationId)
        {
            _playerNationId = nationId;
        }

        /// <summary>
        /// 城池佔領
        /// </summary>
        public void CaptureCity(string cityId, string newNationId)
        {
            var city = GetCity(cityId);
            if (city == null) return;

            var previousNationId = city.NationId;

            // 移除原國家控制
            if (!string.IsNullOrEmpty(previousNationId))
            {
                GetNation(previousNationId)?.RemoveCity(cityId);
            }

            // 添加新國家控制
            city.NationId = newNationId;
            if (!string.IsNullOrEmpty(newNationId))
            {
                GetNation(newNationId)?.AddCity(cityId);
            }

            // 發布事件
            EventManager.Instance?.Publish(new CityCapturedEvent
            {
                City = city,
                PreviousNationId = previousNationId,
                NewNationId = newNationId
            });

            Debug.Log($"[NationManager] 城池 {city.CityName} 被 {GetNation(newNationId)?.NationName ?? "中立"} 佔領！");

            // 檢查國家是否戰敗
            if (!string.IsNullOrEmpty(previousNationId))
            {
                var previousNation = GetNation(previousNationId);
                if (previousNation?.CheckDefeat() == true)
                {
                    EventManager.Instance?.Publish(new NationDefeatedEvent
                    {
                        Nation = previousNation
                    });
                }
            }
        }

        /// <summary>
        /// 升級城池設施
        /// </summary>
        public bool UpgradeFacility(string cityId, CityFacilityType facilityType)
        {
            var city = GetCity(cityId);
            if (city == null) return false;

            var facility = city.GetFacility(facilityType);
            if (facility == null || facility.IsMaxLevel)
            {
                Debug.LogWarning($"[NationManager] 無法升級設施 {facilityType}");
                return false;
            }

            // TODO: 檢查並消耗資源
            var cost = facility.GetUpgradeCost();

            facility.Level++;
            Debug.Log($"[NationManager] {city.CityName} 的 {facilityType} 升級到 Lv{facility.Level}");

            return true;
        }

        /// <summary>
        /// 獲取國家排名
        /// </summary>
        public List<NationData> GetNationRanking()
        {
            return _nations.Values
                .Where(n => n.State != NationState.Defeated)
                .OrderByDescending(n => n.GetPowerRating())
                .ToList();
        }

        /// <summary>
        /// 計算國家總軍力
        /// </summary>
        public void UpdateNationMilitaryPower(string nationId, int totalPower)
        {
            var nation = GetNation(nationId);
            if (nation != null)
            {
                nation.TotalMilitaryPower = totalPower;
            }
        }

        /// <summary>
        /// 獲取中立城池
        /// </summary>
        public List<CityData> GetNeutralCities()
        {
            return _cities.Values
                .Where(c => string.IsNullOrEmpty(c.NationId))
                .ToList();
        }

        /// <summary>
        /// 檢查是否有國家統一天下
        /// </summary>
        public NationData CheckUnification()
        {
            var activeNations = _nations.Values
                .Where(n => n.State == NationState.Active)
                .ToList();

            // 只剩一個活躍國家
            if (activeNations.Count == 1)
            {
                return activeNations[0];
            }

            // 或者一個國家控制了所有首都
            var allCapitals = _cities.Values
                .Where(c => c.Tier == CityTier.Capital)
                .ToList();

            foreach (var nation in activeNations)
            {
                if (allCapitals.All(c => c.NationId == nation.NationId))
                {
                    return nation;
                }
            }

            return null;
        }
    }

    #region 國家相關事件

    /// <summary>
    /// 玩家加入國家事件
    /// </summary>
    public class PlayerJoinedNationEvent : Core.Events.GameEvent
    {
        public string PlayerId;
        public string NationId;
    }

    /// <summary>
    /// 城池被佔領事件
    /// </summary>
    public class CityCapturedEvent : Core.Events.GameEvent
    {
        public CityData City;
        public string PreviousNationId;
        public string NewNationId;
    }

    /// <summary>
    /// 國家戰敗事件
    /// </summary>
    public class NationDefeatedEvent : Core.Events.GameEvent
    {
        public NationData Nation;
    }

    /// <summary>
    /// 天下統一事件
    /// </summary>
    public class UnificationEvent : Core.Events.GameEvent
    {
        public NationData WinningNation;
    }

    #endregion
}

