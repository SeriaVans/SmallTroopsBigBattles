using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Battle
{
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
    /// 陣營角色
    /// </summary>
    public enum FactionRole
    {
        Attacker,   // 攻方
        Defender,   // 守方
        Third       // 第三方
    }

    /// <summary>
    /// 戰場結果
    /// </summary>
    public enum BattleResult
    {
        Ongoing,            // 進行中
        AttackerWin,        // 攻方勝利（城池淪陷）
        DefenderWin,        // 守方勝利（防守成功）
        ThirdPartyWin,      // 第三方勝利
        Draw                // 平局（超時）
    }

    /// <summary>
    /// 戰場陣營
    /// </summary>
    [Serializable]
    public class BattleFaction
    {
        /// <summary>國家 ID</summary>
        public string NationId;

        /// <summary>陣營角色</summary>
        public FactionRole Role;

        /// <summary>入口位置</summary>
        public Vector2 SpawnPoint;

        /// <summary>撤退目標位置</summary>
        public Vector2 RetreatPoint;

        /// <summary>總投入兵力</summary>
        public int TotalSoldiers;

        /// <summary>存活兵力</summary>
        public int AliveSoldiers;

        /// <summary>陣亡兵力</summary>
        public int DeadSoldiers;

        /// <summary>參戰玩家 ID 列表</summary>
        public List<string> PlayerIds = new List<string>();

        /// <summary>士兵池</summary>
        public BattlefieldSoldierPool SoldierPool;

        public BattleFaction()
        {
            SoldierPool = new BattlefieldSoldierPool();
            PlayerIds = new List<string>();
        }

        public BattleFaction(string nationId, FactionRole role, Vector2 spawnPoint)
        {
            NationId = nationId;
            Role = role;
            SpawnPoint = spawnPoint;
            RetreatPoint = spawnPoint; // 撤退到入口
            SoldierPool = new BattlefieldSoldierPool { NationId = nationId };
            PlayerIds = new List<string>();
        }

        /// <summary>
        /// 添加參戰玩家
        /// </summary>
        public void AddPlayer(string playerId)
        {
            if (!PlayerIds.Contains(playerId))
            {
                PlayerIds.Add(playerId);
            }
        }

        /// <summary>
        /// 記錄士兵投入
        /// </summary>
        public void RecordSoldierDeployed(int count)
        {
            TotalSoldiers += count;
            AliveSoldiers += count;
        }

        /// <summary>
        /// 記錄士兵陣亡
        /// </summary>
        public void RecordSoldierDeath()
        {
            DeadSoldiers++;
            AliveSoldiers = Mathf.Max(0, AliveSoldiers - 1);
        }
    }

    /// <summary>
    /// 士兵池
    /// </summary>
    [Serializable]
    public class BattlefieldSoldierPool
    {
        /// <summary>戰場上限</summary>
        public const int MaxOnField = 1000;

        /// <summary>每次補充上限</summary>
        public const int RefillBatchSize = 50;

        /// <summary>國家 ID</summary>
        public string NationId;

        /// <summary>等待中的士兵隊列 (FIFO)</summary>
        public Queue<PooledSoldier> WaitingQueue = new Queue<PooledSoldier>();

        /// <summary>當前戰場上的士兵數量</summary>
        public int CurrentOnField;

        /// <summary>等待中的總數</summary>
        public int TotalWaiting => WaitingQueue.Count;

        /// <summary>是否可以進入戰場</summary>
        public bool CanEnterField => CurrentOnField < MaxOnField;

        /// <summary>需要補充的數量</summary>
        public int NeedRefillCount => Mathf.Min(MaxOnField - CurrentOnField, RefillBatchSize);

        /// <summary>
        /// 添加士兵到池中
        /// </summary>
        public void AddToPool(string playerId, SoldierType type)
        {
            WaitingQueue.Enqueue(new PooledSoldier(playerId, type));
        }

        /// <summary>
        /// 從池中取出士兵
        /// </summary>
        public PooledSoldier DequeueFromPool()
        {
            return WaitingQueue.Count > 0 ? WaitingQueue.Dequeue() : null;
        }

        /// <summary>
        /// 記錄士兵進入戰場
        /// </summary>
        public void OnSoldierEnterField()
        {
            CurrentOnField++;
        }

        /// <summary>
        /// 記錄士兵離開戰場（死亡或撤退）
        /// </summary>
        public void OnSoldierLeaveField()
        {
            CurrentOnField = Mathf.Max(0, CurrentOnField - 1);
        }

        /// <summary>
        /// 獲取指定玩家在池中等待的士兵數量
        /// </summary>
        public Dictionary<SoldierType, int> GetPlayerWaitingCount(string playerId)
        {
            var result = new Dictionary<SoldierType, int>();

            foreach (var soldier in WaitingQueue)
            {
                if (soldier.PlayerId == playerId)
                {
                    if (!result.ContainsKey(soldier.Type))
                        result[soldier.Type] = 0;
                    result[soldier.Type]++;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// 戰場實例
    /// </summary>
    [Serializable]
    public class Battlefield
    {
        /// <summary>戰場唯一 ID</summary>
        public string BattlefieldId;

        /// <summary>戰鬥所在城池 ID</summary>
        public string CityId;

        /// <summary>戰鬥所在節點 ID</summary>
        public string NodeId;

        /// <summary>戰場狀態</summary>
        public BattleState State;

        /// <summary>戰鬥結果</summary>
        public BattleResult Result;

        /// <summary>開始時間</summary>
        public DateTime StartTime;

        /// <summary>結束時間</summary>
        public DateTime? EndTime;

        /// <summary>戰鬥持續時間（秒）</summary>
        public float Duration;

        /// <summary>戰場尺寸</summary>
        public Vector2 BattlefieldSize = new Vector2(3000, 2000);

        /// <summary>參戰陣營</summary>
        public Dictionary<string, BattleFaction> Factions = new Dictionary<string, BattleFaction>();

        /// <summary>所有士兵列表</summary>
        public List<BattleSoldier> Soldiers = new List<BattleSoldier>();

        /// <summary>城牆最大血量</summary>
        public float WallMaxHp;

        /// <summary>城牆當前血量</summary>
        public float WallCurrentHp;

        /// <summary>最大戰鬥時間（秒）</summary>
        public float MaxBattleDuration = 1800f; // 30 分鐘

        /// <summary>城牆是否被摧毀</summary>
        public bool IsWallDestroyed => WallCurrentHp <= 0;

        /// <summary>戰場中心位置</summary>
        public Vector2 Center => BattlefieldSize / 2f;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Battlefield()
        {
            BattlefieldId = Guid.NewGuid().ToString();
            State = BattleState.Preparing;
            Result = BattleResult.Ongoing;
            Factions = new Dictionary<string, BattleFaction>();
            Soldiers = new List<BattleSoldier>();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public Battlefield(string cityId, string attackerNationId, string defenderNationId, float wallHp)
        {
            BattlefieldId = Guid.NewGuid().ToString();
            CityId = cityId;
            State = BattleState.Preparing;
            Result = BattleResult.Ongoing;
            StartTime = DateTime.Now;
            WallMaxHp = wallHp;
            WallCurrentHp = wallHp;

            Factions = new Dictionary<string, BattleFaction>();
            Soldiers = new List<BattleSoldier>();

            // 設置攻守雙方入口位置
            var attackerSpawn = new Vector2(100, BattlefieldSize.y / 2);  // 左側
            var defenderSpawn = new Vector2(BattlefieldSize.x - 100, BattlefieldSize.y / 2);  // 右側

            AddFaction(attackerNationId, FactionRole.Attacker, attackerSpawn);
            AddFaction(defenderNationId, FactionRole.Defender, defenderSpawn);
        }

        /// <summary>
        /// 添加陣營
        /// </summary>
        public void AddFaction(string nationId, FactionRole role, Vector2 spawnPoint)
        {
            if (!Factions.ContainsKey(nationId))
            {
                Factions[nationId] = new BattleFaction(nationId, role, spawnPoint);
            }
        }

        /// <summary>
        /// 添加第三方介入
        /// </summary>
        public void AddThirdParty(string nationId)
        {
            if (Factions.ContainsKey(nationId)) return;

            // 第三方從上方進入
            var thirdPartySpawn = new Vector2(BattlefieldSize.x / 2, BattlefieldSize.y - 100);
            AddFaction(nationId, FactionRole.Third, thirdPartySpawn);
        }

        /// <summary>
        /// 獲取陣營
        /// </summary>
        public BattleFaction GetFaction(string nationId)
        {
            return Factions.TryGetValue(nationId, out var faction) ? faction : null;
        }

        /// <summary>
        /// 獲取攻方陣營
        /// </summary>
        public BattleFaction GetAttacker()
        {
            return Factions.Values.FirstOrDefault(f => f.Role == FactionRole.Attacker);
        }

        /// <summary>
        /// 獲取守方陣營
        /// </summary>
        public BattleFaction GetDefender()
        {
            return Factions.Values.FirstOrDefault(f => f.Role == FactionRole.Defender);
        }

        /// <summary>
        /// 添加士兵到戰場
        /// </summary>
        public BattleSoldier AddSoldier(SoldierType type, string playerId, string nationId)
        {
            var faction = GetFaction(nationId);
            if (faction == null) return null;

            var soldier = new BattleSoldier(type, playerId, nationId, faction.SpawnPoint);
            Soldiers.Add(soldier);
            faction.RecordSoldierDeployed(1);
            faction.SoldierPool.OnSoldierEnterField();

            return soldier;
        }

        /// <summary>
        /// 移除士兵（死亡）
        /// </summary>
        public void RemoveSoldier(BattleSoldier soldier)
        {
            soldier.State = SoldierState.Dead;

            var faction = GetFaction(soldier.NationId);
            if (faction != null)
            {
                faction.RecordSoldierDeath();
                faction.SoldierPool.OnSoldierLeaveField();
            }
        }

        /// <summary>
        /// 對城牆造成傷害
        /// </summary>
        public void DamageWall(float damage)
        {
            WallCurrentHp = Mathf.Max(0, WallCurrentHp - damage);
        }

        /// <summary>
        /// 獲取指定國家的存活士兵
        /// </summary>
        public List<BattleSoldier> GetAliveSoldiersByNation(string nationId)
        {
            return Soldiers.Where(s => s.NationId == nationId && s.IsAlive).ToList();
        }

        /// <summary>
        /// 獲取指定玩家的存活士兵
        /// </summary>
        public List<BattleSoldier> GetAliveSoldiersByPlayer(string playerId)
        {
            return Soldiers.Where(s => s.OwnerId == playerId && s.IsAlive).ToList();
        }

        /// <summary>
        /// 獲取敵方士兵
        /// </summary>
        public List<BattleSoldier> GetEnemySoldiers(string nationId)
        {
            return Soldiers.Where(s => s.NationId != nationId && s.IsAlive).ToList();
        }

        /// <summary>
        /// 檢查戰鬥是否結束
        /// </summary>
        public BattleResult CheckBattleEnd()
        {
            // 城牆被摧毀 - 攻方勝利
            if (IsWallDestroyed)
            {
                return BattleResult.AttackerWin;
            }

            // 檢查各方存活士兵
            var attacker = GetAttacker();
            var defender = GetDefender();

            int attackerAlive = GetAliveSoldiersByNation(attacker?.NationId)?.Count ?? 0;
            int defenderAlive = GetAliveSoldiersByNation(defender?.NationId)?.Count ?? 0;

            // 攻方全滅且沒有後援 - 守方勝利
            if (attackerAlive == 0 && attacker?.SoldierPool.TotalWaiting == 0)
            {
                return BattleResult.DefenderWin;
            }

            // 守方全滅且城牆未破 - 檢查是否還有攻擊者
            if (defenderAlive == 0 && defender?.SoldierPool.TotalWaiting == 0)
            {
                // 攻方仍在，繼續攻城
                if (attackerAlive > 0)
                {
                    return BattleResult.Ongoing;
                }
            }

            // 超時 - 守方勝利
            Duration = (float)(DateTime.Now - StartTime).TotalSeconds;
            if (Duration >= MaxBattleDuration)
            {
                return BattleResult.DefenderWin;
            }

            return BattleResult.Ongoing;
        }

        /// <summary>
        /// 開始戰鬥
        /// </summary>
        public void StartBattle()
        {
            State = BattleState.Fighting;
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// 結束戰鬥
        /// </summary>
        public void EndBattle(BattleResult result)
        {
            State = BattleState.Ended;
            Result = result;
            EndTime = DateTime.Now;
            Duration = (float)(EndTime.Value - StartTime).TotalSeconds;
        }

        /// <summary>
        /// 生成戰場快照
        /// </summary>
        public BattleSnapshot CreateSnapshot()
        {
            return new BattleSnapshot(this);
        }
    }

    /// <summary>
    /// 戰場快照（用於網路同步）
    /// </summary>
    [Serializable]
    public class BattleSnapshot
    {
        public string BattlefieldId;
        public long Timestamp;
        public BattleState State;
        public float WallHpPercent;
        public Dictionary<string, int> FactionAliveCount;
        public List<SoldierSnapshot> Soldiers;

        public BattleSnapshot() { }

        public BattleSnapshot(Battlefield battlefield)
        {
            BattlefieldId = battlefield.BattlefieldId;
            Timestamp = DateTime.Now.Ticks;
            State = battlefield.State;
            WallHpPercent = battlefield.WallCurrentHp / battlefield.WallMaxHp;

            FactionAliveCount = new Dictionary<string, int>();
            foreach (var faction in battlefield.Factions)
            {
                FactionAliveCount[faction.Key] = faction.Value.AliveSoldiers;
            }

            Soldiers = battlefield.Soldiers
                .Where(s => s.IsAlive)
                .Select(s => new SoldierSnapshot(s))
                .ToList();
        }
    }
}

