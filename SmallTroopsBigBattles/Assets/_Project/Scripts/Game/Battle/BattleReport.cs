using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 戰報數據
    /// </summary>
    [Serializable]
    public class BattleReport
    {
        /// <summary>戰報 ID</summary>
        public string ReportId;

        /// <summary>戰場 ID</summary>
        public string BattlefieldId;

        /// <summary>城池 ID</summary>
        public string CityId;

        /// <summary>城池名稱</summary>
        public string CityName;

        /// <summary>戰鬥結果</summary>
        public BattleResult Result;

        /// <summary>開始時間</summary>
        public DateTime StartTime;

        /// <summary>結束時間</summary>
        public DateTime EndTime;

        /// <summary>持續時間（秒）</summary>
        public float Duration;

        /// <summary>各陣營報告</summary>
        public List<FactionReport> Factions = new List<FactionReport>();

        /// <summary>參戰玩家報告</summary>
        public List<PlayerReport> PlayerReports = new List<PlayerReport>();

        /// <summary>城牆起始血量</summary>
        public float WallInitialHp;

        /// <summary>城牆最終血量</summary>
        public float WallFinalHp;

        /// <summary>
        /// 建構函式
        /// </summary>
        public BattleReport()
        {
            ReportId = Guid.NewGuid().ToString();
            Factions = new List<FactionReport>();
            PlayerReports = new List<PlayerReport>();
        }

        /// <summary>
        /// 從戰場創建戰報
        /// </summary>
        public static BattleReport CreateFromBattlefield(Battlefield battlefield)
        {
            var report = new BattleReport
            {
                ReportId = Guid.NewGuid().ToString(),
                BattlefieldId = battlefield.BattlefieldId,
                CityId = battlefield.CityId,
                Result = battlefield.Result,
                StartTime = battlefield.StartTime,
                EndTime = battlefield.EndTime ?? DateTime.Now,
                Duration = battlefield.Duration,
                WallInitialHp = battlefield.WallMaxHp,
                WallFinalHp = battlefield.WallCurrentHp
            };

            // 添加陣營報告
            foreach (var faction in battlefield.Factions.Values)
            {
                report.Factions.Add(FactionReport.CreateFromFaction(faction));
            }

            // 生成玩家報告
            var playerStats = new Dictionary<string, PlayerReport>();

            foreach (var soldier in battlefield.Soldiers)
            {
                if (!playerStats.ContainsKey(soldier.OwnerId))
                {
                    playerStats[soldier.OwnerId] = new PlayerReport
                    {
                        PlayerId = soldier.OwnerId,
                        NationId = soldier.NationId
                    };
                }

                var playerReport = playerStats[soldier.OwnerId];
                playerReport.TotalDeployed++;

                if (!playerReport.DeployedByType.ContainsKey(soldier.Type))
                    playerReport.DeployedByType[soldier.Type] = 0;
                playerReport.DeployedByType[soldier.Type]++;

                if (!soldier.IsAlive)
                {
                    playerReport.TotalDead++;

                    if (!playerReport.DeadByType.ContainsKey(soldier.Type))
                        playerReport.DeadByType[soldier.Type] = 0;
                    playerReport.DeadByType[soldier.Type]++;
                }
            }

            report.PlayerReports = playerStats.Values.ToList();

            return report;
        }

        /// <summary>
        /// 獲取勝利方國家 ID
        /// </summary>
        public string GetWinnerNationId()
        {
            return Result switch
            {
                BattleResult.AttackerWin => Factions.FirstOrDefault(f => f.Role == FactionRole.Attacker)?.NationId,
                BattleResult.DefenderWin => Factions.FirstOrDefault(f => f.Role == FactionRole.Defender)?.NationId,
                BattleResult.ThirdPartyWin => Factions.FirstOrDefault(f => f.Role == FactionRole.Third)?.NationId,
                _ => null
            };
        }

        /// <summary>
        /// 獲取總投入兵力
        /// </summary>
        public int GetTotalDeployed()
        {
            return Factions.Sum(f => f.TotalDeployed);
        }

        /// <summary>
        /// 獲取總陣亡數
        /// </summary>
        public int GetTotalDead()
        {
            return Factions.Sum(f => f.TotalDead);
        }

        /// <summary>
        /// 生成戰報摘要文字
        /// </summary>
        public string GenerateSummary()
        {
            string resultText = Result switch
            {
                BattleResult.AttackerWin => "攻方勝利 - 城池淪陷",
                BattleResult.DefenderWin => "守方勝利 - 防守成功",
                BattleResult.ThirdPartyWin => "第三方勝利",
                BattleResult.Draw => "平局",
                _ => "戰鬥結束"
            };

            string summary = $"【戰報】\n";
            summary += $"戰役：{CityName ?? CityId}\n";
            summary += $"結果：{resultText}\n";
            summary += $"持續時間：{FormatDuration(Duration)}\n";
            summary += $"\n";

            foreach (var faction in Factions)
            {
                string roleText = faction.Role switch
                {
                    FactionRole.Attacker => "攻方",
                    FactionRole.Defender => "守方",
                    FactionRole.Third => "第三方",
                    _ => ""
                };

                summary += $"【{roleText} - {GetNationName(faction.NationId)}】\n";
                summary += $"  投入兵力：{faction.TotalDeployed}\n";
                summary += $"  陣亡：{faction.TotalDead}\n";
                summary += $"  存活：{faction.TotalSurvived}\n";
                summary += $"\n";
            }

            if (WallInitialHp > 0)
            {
                int wallPercent = Mathf.RoundToInt((WallFinalHp / WallInitialHp) * 100);
                summary += $"城牆狀態：{wallPercent}%\n";
            }

            return summary;
        }

        /// <summary>
        /// 格式化持續時間
        /// </summary>
        private string FormatDuration(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{minutes}分{secs}秒";
        }

        /// <summary>
        /// 獲取國家名稱
        /// </summary>
        private string GetNationName(string nationId)
        {
            return nationId switch
            {
                "nation_shu" => "蜀國",
                "nation_wei" => "魏國",
                "nation_wu" => "吳國",
                _ => nationId
            };
        }
    }

    /// <summary>
    /// 陣營報告
    /// </summary>
    [Serializable]
    public class FactionReport
    {
        /// <summary>國家 ID</summary>
        public string NationId;

        /// <summary>陣營角色</summary>
        public FactionRole Role;

        /// <summary>總投入兵力</summary>
        public int TotalDeployed;

        /// <summary>總陣亡數</summary>
        public int TotalDead;

        /// <summary>總存活數</summary>
        public int TotalSurvived => TotalDeployed - TotalDead;

        /// <summary>參戰玩家數</summary>
        public int PlayerCount;

        /// <summary>按兵種統計投入</summary>
        public Dictionary<SoldierType, int> DeployedByType = new Dictionary<SoldierType, int>();

        /// <summary>按兵種統計陣亡</summary>
        public Dictionary<SoldierType, int> DeadByType = new Dictionary<SoldierType, int>();

        /// <summary>
        /// 從陣營數據創建報告
        /// </summary>
        public static FactionReport CreateFromFaction(BattleFaction faction)
        {
            return new FactionReport
            {
                NationId = faction.NationId,
                Role = faction.Role,
                TotalDeployed = faction.TotalSoldiers,
                TotalDead = faction.DeadSoldiers,
                PlayerCount = faction.PlayerIds.Count
            };
        }
    }

    /// <summary>
    /// 玩家戰報
    /// </summary>
    [Serializable]
    public class PlayerReport
    {
        /// <summary>玩家 ID</summary>
        public string PlayerId;

        /// <summary>所屬國家</summary>
        public string NationId;

        /// <summary>總投入兵力</summary>
        public int TotalDeployed;

        /// <summary>總陣亡數</summary>
        public int TotalDead;

        /// <summary>總存活數</summary>
        public int TotalSurvived => TotalDeployed - TotalDead;

        /// <summary>按兵種統計投入</summary>
        public Dictionary<SoldierType, int> DeployedByType = new Dictionary<SoldierType, int>();

        /// <summary>按兵種統計陣亡</summary>
        public Dictionary<SoldierType, int> DeadByType = new Dictionary<SoldierType, int>();

        /// <summary>擊殺數</summary>
        public int Kills;

        /// <summary>貢獻值</summary>
        public int Contribution;

        /// <summary>是否獲得獎勵</summary>
        public bool HasReward;

        /// <summary>獎勵內容</summary>
        public BattleReward Reward;
    }

    /// <summary>
    /// 戰鬥獎勵
    /// </summary>
    [Serializable]
    public class BattleReward
    {
        /// <summary>銅錢獎勵</summary>
        public int Copper;

        /// <summary>經驗獎勵</summary>
        public int Experience;

        /// <summary>聲望獎勵</summary>
        public int Prestige;

        /// <summary>其他獎勵描述</summary>
        public string Description;
    }

    /// <summary>
    /// 戰報管理器
    /// </summary>
    public static class BattleReportManager
    {
        /// <summary>戰報緩存（最近 100 條）</summary>
        private static readonly LinkedList<BattleReport> _reportCache = new LinkedList<BattleReport>();

        /// <summary>緩存上限</summary>
        private const int MaxCacheSize = 100;

        /// <summary>
        /// 保存戰報
        /// </summary>
        public static void SaveReport(BattleReport report)
        {
            _reportCache.AddFirst(report);

            // 限制緩存大小
            while (_reportCache.Count > MaxCacheSize)
            {
                _reportCache.RemoveLast();
            }

            Debug.Log($"[BattleReportManager] 保存戰報：{report.ReportId}");
        }

        /// <summary>
        /// 獲取玩家的戰報列表
        /// </summary>
        public static List<BattleReport> GetPlayerReports(string playerId, int limit = 20)
        {
            return _reportCache
                .Where(r => r.PlayerReports.Any(p => p.PlayerId == playerId))
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 獲取最近的戰報
        /// </summary>
        public static List<BattleReport> GetRecentReports(int limit = 20)
        {
            return _reportCache.Take(limit).ToList();
        }

        /// <summary>
        /// 根據 ID 獲取戰報
        /// </summary>
        public static BattleReport GetReport(string reportId)
        {
            return _reportCache.FirstOrDefault(r => r.ReportId == reportId);
        }

        /// <summary>
        /// 清除所有緩存
        /// </summary>
        public static void ClearCache()
        {
            _reportCache.Clear();
        }
    }
}

