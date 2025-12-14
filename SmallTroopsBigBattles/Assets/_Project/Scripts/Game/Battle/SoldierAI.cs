using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 士兵 AI 系統 - 控制士兵的自動行為
    /// </summary>
    public static class SoldierAI
    {
        /// <summary>
        /// 更新士兵 AI
        /// </summary>
        /// <param name="soldier">要更新的士兵</param>
        /// <param name="battlefield">戰場實例</param>
        /// <param name="deltaTime">時間間隔</param>
        public static void UpdateSoldier(BattleSoldier soldier, Battlefield battlefield, float deltaTime)
        {
            if (!soldier.IsAlive) return;

            // 更新攻擊冷卻
            soldier.UpdateCooldown(deltaTime);

            // 根據狀態執行不同行為
            switch (soldier.State)
            {
                case SoldierState.Idle:
                    HandleIdleState(soldier, battlefield);
                    break;

                case SoldierState.Advancing:
                    HandleAdvancingState(soldier, battlefield, deltaTime);
                    break;

                case SoldierState.Moving:
                    HandleMovingState(soldier, battlefield, deltaTime);
                    break;

                case SoldierState.Attacking:
                    HandleAttackingState(soldier, battlefield, deltaTime);
                    break;

                case SoldierState.Retreating:
                    HandleRetreatingState(soldier, battlefield, deltaTime);
                    break;
            }
        }

        /// <summary>
        /// 處理閒置狀態
        /// </summary>
        private static void HandleIdleState(BattleSoldier soldier, Battlefield battlefield)
        {
            // 尋找目標
            var enemies = battlefield.GetEnemySoldiers(soldier.NationId);
            var target = BattleCalculator.SelectTarget(soldier, enemies);

            if (target != null)
            {
                soldier.SetTarget(target);
            }
            else
            {
                // 沒有目標，開始前進
                soldier.State = SoldierState.Advancing;
            }
        }

        /// <summary>
        /// 處理前進狀態
        /// </summary>
        private static void HandleAdvancingState(BattleSoldier soldier, Battlefield battlefield, float deltaTime)
        {
            // 檢查是否有敵人進入攻擊範圍
            var enemies = battlefield.GetEnemySoldiers(soldier.NationId);
            var nearestEnemy = FindNearestEnemy(soldier, enemies);

            if (nearestEnemy != null)
            {
                float distance = soldier.GetDistanceTo(nearestEnemy.Position);

                // 弓兵特殊處理：保持距離
                if (soldier.Type == SoldierType.Archer)
                {
                    if (distance <= soldier.AttackRange)
                    {
                        soldier.SetTarget(nearestEnemy);
                        soldier.State = SoldierState.Attacking;
                        return;
                    }
                }
                else
                {
                    // 近戰兵種：進入攻擊範圍就開始攻擊
                    if (distance <= soldier.AttackRange)
                    {
                        soldier.SetTarget(nearestEnemy);
                        soldier.State = SoldierState.Attacking;
                        return;
                    }

                    // 發現敵人，追擊
                    if (distance < 30f)
                    {
                        soldier.SetTarget(nearestEnemy);
                        return;
                    }
                }
            }

            // 繼續前進
            Vector2 direction = BattleCalculator.GetAdvanceDirection(soldier, battlefield);
            soldier.Move(direction, deltaTime);
            soldier.Position = BattleCalculator.ClampToBattlefield(soldier.Position, battlefield);

            // 攻方到達城牆區域，攻擊城牆
            if (BattleCalculator.IsInSiegeRange(soldier, battlefield))
            {
                soldier.State = SoldierState.Attacking;
                soldier.TargetId = -1; // 特殊標記：攻擊城牆
            }
        }

        /// <summary>
        /// 處理移動狀態（追擊目標）
        /// </summary>
        private static void HandleMovingState(BattleSoldier soldier, Battlefield battlefield, float deltaTime)
        {
            // 檢查目標是否還有效
            var target = FindSoldierById(battlefield, soldier.TargetId);

            if (target == null || !target.IsAlive)
            {
                // 目標無效，重新尋找
                var enemies = battlefield.GetEnemySoldiers(soldier.NationId);
                target = BattleCalculator.SelectTarget(soldier, enemies);

                if (target != null)
                {
                    soldier.SetTarget(target);
                }
                else
                {
                    soldier.ClearTarget();
                    return;
                }
            }

            float distance = soldier.GetDistanceTo(target.Position);

            // 弓兵保持距離
            if (soldier.Type == SoldierType.Archer)
            {
                if (distance <= soldier.AttackRange)
                {
                    soldier.State = SoldierState.Attacking;

                    // 檢查是否需要後退
                    var kiteDirection = BattleCalculator.GetArcherKiteDirection(soldier, target);
                    if (kiteDirection != Vector2.zero)
                    {
                        soldier.Move(kiteDirection, deltaTime * 0.5f); // 後退速度較慢
                    }
                    return;
                }
            }
            else
            {
                // 近戰單位
                if (distance <= soldier.AttackRange)
                {
                    soldier.State = SoldierState.Attacking;
                    return;
                }
            }

            // 追擊目標
            Vector2 direction = BattleCalculator.GetChaseDirection(soldier, target);
            soldier.Move(direction, deltaTime);
            soldier.Position = BattleCalculator.ClampToBattlefield(soldier.Position, battlefield);
            soldier.TargetPosition = target.Position;
        }

        /// <summary>
        /// 處理攻擊狀態
        /// </summary>
        private static void HandleAttackingState(BattleSoldier soldier, Battlefield battlefield, float deltaTime)
        {
            // 攻擊城牆
            if (soldier.TargetId == -1)
            {
                HandleSiegeAttack(soldier, battlefield);
                return;
            }

            // 攻擊敵方士兵
            var target = FindSoldierById(battlefield, soldier.TargetId);

            if (target == null || !target.IsAlive)
            {
                // 目標死亡，尋找新目標
                soldier.ClearTarget();
                return;
            }

            float distance = soldier.GetDistanceTo(target.Position);

            // 檢查是否在攻擊範圍內
            if (distance > soldier.AttackRange * 1.2f)
            {
                // 目標脫離攻擊範圍，追擊
                soldier.State = SoldierState.Moving;
                return;
            }

            // 執行攻擊
            if (soldier.CanAttack)
            {
                float damage = BattleCalculator.CalculateDamage(soldier, target);
                target.TakeDamage(damage, soldier.Type);
                soldier.ResetAttackCooldown();

                // 目標死亡
                if (!target.IsAlive)
                {
                    battlefield.RemoveSoldier(target);
                    soldier.ClearTarget();
                }
            }

            // 弓兵在攻擊時也要保持距離
            if (soldier.Type == SoldierType.Archer)
            {
                var kiteDirection = BattleCalculator.GetArcherKiteDirection(soldier, target);
                if (kiteDirection != Vector2.zero)
                {
                    soldier.Move(kiteDirection, deltaTime * 0.3f);
                    soldier.Position = BattleCalculator.ClampToBattlefield(soldier.Position, battlefield);
                }
            }
        }

        /// <summary>
        /// 處理攻城攻擊
        /// </summary>
        private static void HandleSiegeAttack(BattleSoldier soldier, Battlefield battlefield)
        {
            if (!soldier.CanAttack) return;

            // 計算攻城傷害
            float siegeDamage = BattleCalculator.CalculateSiegeDamage(soldier);
            battlefield.DamageWall(siegeDamage);
            soldier.ResetAttackCooldown();

            // 檢查周圍是否有敵人
            var enemies = battlefield.GetEnemySoldiers(soldier.NationId);
            var nearestEnemy = FindNearestEnemy(soldier, enemies);

            if (nearestEnemy != null && soldier.GetDistanceTo(nearestEnemy.Position) < soldier.AttackRange * 2)
            {
                // 附近有敵人，優先攻擊敵人
                soldier.SetTarget(nearestEnemy);
            }
        }

        /// <summary>
        /// 處理撤退狀態
        /// </summary>
        private static void HandleRetreatingState(BattleSoldier soldier, Battlefield battlefield, float deltaTime)
        {
            // 撤退中不攻擊
            Vector2 direction = BattleCalculator.GetRetreatDirection(soldier, battlefield);

            // 撤退速度根據兵種不同
            float retreatSpeedMultiplier = soldier.Type switch
            {
                SoldierType.Cavalry => 1.2f,  // 騎兵撤退較快
                SoldierType.Archer => 1.0f,
                _ => 0.8f                      // 步兵撤退較慢
            };

            soldier.Move(direction, deltaTime * retreatSpeedMultiplier);
            soldier.Position = BattleCalculator.ClampToBattlefield(soldier.Position, battlefield);

            // 檢查是否到達撤退點
            if (BattleCalculator.HasReachedRetreatPoint(soldier, battlefield))
            {
                // 成功撤退
                soldier.State = SoldierState.Dead; // 從戰場移除
                var faction = battlefield.GetFaction(soldier.NationId);
                faction?.SoldierPool.OnSoldierLeaveField();

                // TODO: 記錄撤退成功，返回玩家領地
            }
        }

        #region 輔助方法

        /// <summary>
        /// 根據 ID 查找士兵
        /// </summary>
        private static BattleSoldier FindSoldierById(Battlefield battlefield, long soldierId)
        {
            foreach (var soldier in battlefield.Soldiers)
            {
                if (soldier.SoldierId == soldierId)
                    return soldier;
            }
            return null;
        }

        /// <summary>
        /// 查找最近的敵人
        /// </summary>
        private static BattleSoldier FindNearestEnemy(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            BattleSoldier nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = soldier.GetDistanceTo(enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        #endregion
    }
}

