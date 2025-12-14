using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 戰鬥管理器 - 負責管理所有戰場實例
    /// </summary>
    public class BattleManager : SingletonBase<BattleManager>
    {
        [Header("戰鬥設定")]
        [SerializeField] private float battleTickRate = 0.1f;  // 每 0.1 秒更新一次
        [SerializeField] private float refillInterval = 1f;    // 每秒補充士兵

        /// <summary>所有活躍的戰場</summary>
        private Dictionary<string, Battlefield> _activeBattles = new Dictionary<string, Battlefield>();

        /// <summary>當前觀戰的戰場 ID</summary>
        private string _currentSpectatingBattleId;

        /// <summary>戰鬥更新計時器</summary>
        private float _battleTickTimer;

        /// <summary>士兵補充計時器</summary>
        private float _refillTimer;

        /// <summary>活躍戰場列表（唯讀）</summary>
        public IReadOnlyDictionary<string, Battlefield> ActiveBattles => _activeBattles;

        /// <summary>當前觀戰的戰場</summary>
        public Battlefield CurrentSpectatingBattle =>
            !string.IsNullOrEmpty(_currentSpectatingBattleId) &&
            _activeBattles.TryGetValue(_currentSpectatingBattleId, out var battle)
                ? battle
                : null;

        private void Update()
        {
            if (_activeBattles.Count == 0) return;

            // 戰鬥邏輯更新
            _battleTickTimer += Time.deltaTime;
            if (_battleTickTimer >= battleTickRate)
            {
                UpdateAllBattles(_battleTickTimer);
                _battleTickTimer = 0;
            }

            // 士兵補充更新
            _refillTimer += Time.deltaTime;
            if (_refillTimer >= refillInterval)
            {
                RefillAllBattles();
                _refillTimer = 0;
            }
        }

        #region 戰場管理

        /// <summary>
        /// 創建新戰場
        /// </summary>
        public Battlefield CreateBattle(string cityId, string attackerNationId, string defenderNationId, float wallHp)
        {
            // 檢查該城池是否已有戰鬥
            var existingBattle = GetBattleByCity(cityId);
            if (existingBattle != null)
            {
                Debug.LogWarning($"[BattleManager] 城池 {cityId} 已有戰鬥進行中");
                return existingBattle;
            }

            var battlefield = new Battlefield(cityId, attackerNationId, defenderNationId, wallHp);
            _activeBattles[battlefield.BattlefieldId] = battlefield;

            // 發布戰鬥創建事件
            EventManager.Instance?.Publish(new BattleCreatedEvent
            {
                Battlefield = battlefield
            });

            Debug.Log($"[BattleManager] 創建戰場：{battlefield.BattlefieldId}，城池：{cityId}");
            return battlefield;
        }

        /// <summary>
        /// 開始戰鬥
        /// </summary>
        public void StartBattle(string battlefieldId)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            battle.StartBattle();

            EventManager.Instance?.Publish(new BattleStartedEvent
            {
                Battlefield = battle
            });

            Debug.Log($"[BattleManager] 戰鬥開始：{battlefieldId}");
        }

        /// <summary>
        /// 結束戰鬥
        /// </summary>
        public void EndBattle(string battlefieldId, BattleResult result)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            battle.EndBattle(result);

            // 生成並保存戰報
            var report = BattleReport.CreateFromBattlefield(battle);
            BattleReportManager.SaveReport(report);

            EventManager.Instance?.Publish(new BattleEndedEvent
            {
                Battlefield = battle,
                Result = result,
                Report = report
            });

            Debug.Log($"[BattleManager] 戰鬥結束：{battlefieldId}，結果：{result}");
            Debug.Log(report.GenerateSummary());

            // 延遲移除戰場（讓玩家有時間查看結果）
            // TODO: 使用協程延遲移除
            _activeBattles.Remove(battlefieldId);
        }

        /// <summary>
        /// 根據城池獲取戰場
        /// </summary>
        public Battlefield GetBattleByCity(string cityId)
        {
            return _activeBattles.Values.FirstOrDefault(b => b.CityId == cityId);
        }

        /// <summary>
        /// 根據 ID 獲取戰場
        /// </summary>
        public Battlefield GetBattle(string battlefieldId)
        {
            return _activeBattles.TryGetValue(battlefieldId, out var battle) ? battle : null;
        }

        #endregion

        #region 士兵管理

        /// <summary>
        /// 派兵進入戰場
        /// </summary>
        public void DeploySoldiers(string battlefieldId, string playerId, string nationId,
            Dictionary<SoldierType, int> soldiers)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            var faction = battle.GetFaction(nationId);
            if (faction == null)
            {
                Debug.LogError($"[BattleManager] 陣營不存在：{nationId}");
                return;
            }

            // 添加玩家到參戰列表
            faction.AddPlayer(playerId);

            int totalDeployed = 0;
            int totalPooled = 0;

            foreach (var kvp in soldiers)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    if (faction.SoldierPool.CanEnterField)
                    {
                        // 直接進入戰場
                        battle.AddSoldier(kvp.Key, playerId, nationId);
                        totalDeployed++;
                    }
                    else
                    {
                        // 加入士兵池等待
                        faction.SoldierPool.AddToPool(playerId, kvp.Key);
                        totalPooled++;
                    }
                }
            }

            Debug.Log($"[BattleManager] 玩家 {playerId} 派兵：{totalDeployed} 進入戰場，{totalPooled} 進入等待池");

            // 發布派兵事件
            EventManager.Instance?.Publish(new SoldiersDeployedEvent
            {
                BattlefieldId = battlefieldId,
                PlayerId = playerId,
                NationId = nationId,
                DeployedCount = totalDeployed,
                PooledCount = totalPooled
            });
        }

        /// <summary>
        /// 玩家撤退
        /// </summary>
        public void PlayerRetreat(string battlefieldId, string playerId)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            // 將該玩家的所有士兵設為撤退狀態
            var playerSoldiers = battle.GetAliveSoldiersByPlayer(playerId);
            foreach (var soldier in playerSoldiers)
            {
                soldier.StartRetreat();
            }

            // 從士兵池中移除該玩家的等待士兵
            // TODO: 實現從士兵池移除

            Debug.Log($"[BattleManager] 玩家 {playerId} 開始撤退，{playerSoldiers.Count} 名士兵");

            EventManager.Instance?.Publish(new PlayerRetreatEvent
            {
                BattlefieldId = battlefieldId,
                PlayerId = playerId,
                RetreatingCount = playerSoldiers.Count
            });
        }

        #endregion

        #region 觀戰系統

        /// <summary>
        /// 開始觀戰
        /// </summary>
        public void StartSpectating(string battlefieldId)
        {
            if (!_activeBattles.ContainsKey(battlefieldId))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            _currentSpectatingBattleId = battlefieldId;

            EventManager.Instance?.Publish(new SpectatingStartedEvent
            {
                BattlefieldId = battlefieldId
            });
        }

        /// <summary>
        /// 停止觀戰
        /// </summary>
        public void StopSpectating()
        {
            _currentSpectatingBattleId = null;

            EventManager.Instance?.Publish(new SpectatingStoppedEvent());
        }

        /// <summary>
        /// 獲取玩家戰場狀態
        /// </summary>
        public PlayerBattleStatus GetPlayerBattleStatus(string battlefieldId, string playerId)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
                return null;

            var status = new PlayerBattleStatus { PlayerId = playerId };

            // 統計戰場上的士兵
            var aliveSoldiers = battle.GetAliveSoldiersByPlayer(playerId);
            foreach (var soldier in aliveSoldiers)
            {
                if (!status.OnFieldSoldiers.ContainsKey(soldier.Type))
                    status.OnFieldSoldiers[soldier.Type] = 0;
                status.OnFieldSoldiers[soldier.Type]++;
            }

            // 統計士兵池中的士兵
            foreach (var faction in battle.Factions.Values)
            {
                var waitingCount = faction.SoldierPool.GetPlayerWaitingCount(playerId);
                foreach (var kvp in waitingCount)
                {
                    if (!status.WaitingSoldiers.ContainsKey(kvp.Key))
                        status.WaitingSoldiers[kvp.Key] = 0;
                    status.WaitingSoldiers[kvp.Key] += kvp.Value;
                }
            }

            return status;
        }

        #endregion

        #region 戰鬥更新

        /// <summary>
        /// 更新所有戰場
        /// </summary>
        private void UpdateAllBattles(float deltaTime)
        {
            var battleIds = _activeBattles.Keys.ToList();

            foreach (var battleId in battleIds)
            {
                if (!_activeBattles.TryGetValue(battleId, out var battle))
                    continue;

                if (battle.State != BattleState.Fighting)
                    continue;

                UpdateBattle(battle, deltaTime);
            }
        }

        /// <summary>
        /// 更新單個戰場
        /// </summary>
        private void UpdateBattle(Battlefield battlefield, float deltaTime)
        {
            // 更新所有士兵 AI
            foreach (var soldier in battlefield.Soldiers)
            {
                if (soldier.IsAlive)
                {
                    SoldierAI.UpdateSoldier(soldier, battlefield, deltaTime);
                }
            }

            // 檢查戰鬥是否結束
            var result = battlefield.CheckBattleEnd();
            if (result != BattleResult.Ongoing)
            {
                EndBattle(battlefield.BattlefieldId, result);
            }
        }

        /// <summary>
        /// 補充所有戰場的士兵
        /// </summary>
        private void RefillAllBattles()
        {
            foreach (var battle in _activeBattles.Values)
            {
                if (battle.State != BattleState.Fighting)
                    continue;

                RefillBattle(battle);
            }
        }

        /// <summary>
        /// 補充單個戰場的士兵
        /// </summary>
        private void RefillBattle(Battlefield battlefield)
        {
            foreach (var faction in battlefield.Factions.Values)
            {
                int needed = faction.SoldierPool.NeedRefillCount;

                for (int i = 0; i < needed; i++)
                {
                    var pooledSoldier = faction.SoldierPool.DequeueFromPool();
                    if (pooledSoldier == null)
                        break;

                    // 生成士兵進入戰場
                    battlefield.AddSoldier(pooledSoldier.Type, pooledSoldier.PlayerId, faction.NationId);
                }
            }
        }

        #endregion

        #region 第三方介入

        /// <summary>
        /// 第三方加入戰鬥
        /// </summary>
        public void ThirdPartyJoin(string battlefieldId, string thirdPartyNationId)
        {
            if (!_activeBattles.TryGetValue(battlefieldId, out var battle))
            {
                Debug.LogError($"[BattleManager] 找不到戰場：{battlefieldId}");
                return;
            }

            if (battle.Factions.ContainsKey(thirdPartyNationId))
            {
                Debug.LogWarning($"[BattleManager] 國家 {thirdPartyNationId} 已在戰場中");
                return;
            }

            battle.AddThirdParty(thirdPartyNationId);

            EventManager.Instance?.Publish(new ThirdPartyJoinedEvent
            {
                BattlefieldId = battlefieldId,
                NationId = thirdPartyNationId
            });

            Debug.Log($"[BattleManager] 第三方 {thirdPartyNationId} 加入戰場 {battlefieldId}");
        }

        #endregion

        #region 測試方法

        /// <summary>
        /// 創建測試戰場
        /// </summary>
        public Battlefield CreateTestBattle()
        {
            var battle = CreateBattle("test_city", "nation_shu", "nation_wei", 10000);

            // 添加一些測試士兵
            var testSoldiers = new Dictionary<SoldierType, int>
            {
                { SoldierType.Spearman, 50 },
                { SoldierType.Shieldman, 30 },
                { SoldierType.Cavalry, 20 },
                { SoldierType.Archer, 20 }
            };

            DeploySoldiers(battle.BattlefieldId, "test_player_1", "nation_shu", testSoldiers);
            DeploySoldiers(battle.BattlefieldId, "test_player_2", "nation_wei", testSoldiers);

            StartBattle(battle.BattlefieldId);

            return battle;
        }

        #endregion
    }

    #region 玩家戰場狀態

    /// <summary>
    /// 玩家戰場士兵狀態
    /// </summary>
    public class PlayerBattleStatus
    {
        public string PlayerId;
        public string NationId;

        /// <summary>戰場上的士兵（按兵種分類）</summary>
        public Dictionary<SoldierType, int> OnFieldSoldiers = new Dictionary<SoldierType, int>();

        /// <summary>待上陣的士兵（在士兵池中）</summary>
        public Dictionary<SoldierType, int> WaitingSoldiers = new Dictionary<SoldierType, int>();

        /// <summary>戰場上總數</summary>
        public int TotalOnField => OnFieldSoldiers.Values.Sum();

        /// <summary>等待中總數</summary>
        public int TotalWaiting => WaitingSoldiers.Values.Sum();
    }

    #endregion

    #region 戰鬥相關事件

    /// <summary>戰場創建事件</summary>
    public class BattleCreatedEvent : GameEvent
    {
        public Battlefield Battlefield;
    }

    /// <summary>戰鬥開始事件</summary>
    public class BattleStartedEvent : GameEvent
    {
        public Battlefield Battlefield;
    }

    /// <summary>戰鬥結束事件</summary>
    public class BattleEndedEvent : GameEvent
    {
        public Battlefield Battlefield;
        public BattleResult Result;
        public BattleReport Report;
    }

    /// <summary>士兵部署事件</summary>
    public class SoldiersDeployedEvent : GameEvent
    {
        public string BattlefieldId;
        public string PlayerId;
        public string NationId;
        public int DeployedCount;
        public int PooledCount;
    }

    /// <summary>玩家撤退事件</summary>
    public class PlayerRetreatEvent : GameEvent
    {
        public string BattlefieldId;
        public string PlayerId;
        public int RetreatingCount;
    }

    /// <summary>第三方加入事件</summary>
    public class ThirdPartyJoinedEvent : GameEvent
    {
        public string BattlefieldId;
        public string NationId;
    }

    /// <summary>開始觀戰事件</summary>
    public class SpectatingStartedEvent : GameEvent
    {
        public string BattlefieldId;
    }

    /// <summary>停止觀戰事件</summary>
    public class SpectatingStoppedEvent : GameEvent { }

    #endregion
}

