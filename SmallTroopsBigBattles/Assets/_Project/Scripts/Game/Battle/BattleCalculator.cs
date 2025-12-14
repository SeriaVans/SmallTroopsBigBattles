using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 戰鬥計算器 - 處理所有戰鬥相關的數值計算
    /// </summary>
    public static class BattleCalculator
    {
        #region 傷害計算

        /// <summary>
        /// 計算攻擊傷害
        /// </summary>
        /// <param name="attacker">攻擊者</param>
        /// <param name="defender">防禦者</param>
        /// <returns>最終傷害值</returns>
        public static float CalculateDamage(BattleSoldier attacker, BattleSoldier defender)
        {
            // 基礎傷害 = 攻擊力 × (100 / (100 + 敵方防禦))
            float baseDamage = attacker.Attack * (100f / (100f + defender.Defense));

            // 兵種克制係數
            float typeMultiplier = BattleSoldier.GetDamageMultiplier(attacker.Type, defender.Type);

            // 最終傷害 = 基礎傷害 × 克制係數 × 隨機浮動
            float finalDamage = baseDamage * typeMultiplier * Random.Range(0.9f, 1.1f);

            // 撤退中的目標受到額外傷害
            if (defender.State == SoldierState.Retreating)
            {
                finalDamage *= 1.5f;
            }

            return Mathf.Max(1f, finalDamage); // 至少造成 1 點傷害
        }

        /// <summary>
        /// 計算攻城傷害
        /// </summary>
        /// <param name="soldier">攻擊城牆的士兵</param>
        /// <returns>對城牆的傷害</returns>
        public static float CalculateSiegeDamage(BattleSoldier soldier)
        {
            return soldier.Attack * soldier.SiegeDamage;
        }

        #endregion

        #region 目標選擇

        /// <summary>
        /// 為士兵選擇最佳攻擊目標
        /// </summary>
        public static BattleSoldier SelectTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return null;

            // 過濾出存活且在合理範圍內的敵人
            var validTargets = enemies
                .Where(e => e.IsAlive && soldier.GetDistanceTo(e.Position) < 50f)
                .ToList();

            if (validTargets.Count == 0)
                return null;

            // 根據兵種選擇優先目標
            return soldier.Type switch
            {
                SoldierType.Spearman => SelectSpearmanTarget(soldier, validTargets),
                SoldierType.Shieldman => SelectShieldmanTarget(soldier, validTargets),
                SoldierType.Cavalry => SelectCavalryTarget(soldier, validTargets),
                SoldierType.Archer => SelectArcherTarget(soldier, validTargets),
                _ => SelectNearestTarget(soldier, validTargets)
            };
        }

        /// <summary>
        /// 槍兵目標選擇：優先騎兵
        /// </summary>
        private static BattleSoldier SelectSpearmanTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            // 優先選擇騎兵
            var cavalry = enemies.Where(e => e.Type == SoldierType.Cavalry).ToList();
            if (cavalry.Count > 0)
            {
                return SelectNearestTarget(soldier, cavalry);
            }

            return SelectNearestTarget(soldier, enemies);
        }

        /// <summary>
        /// 盾兵目標選擇：優先最近的敵人
        /// </summary>
        private static BattleSoldier SelectShieldmanTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            return SelectNearestTarget(soldier, enemies);
        }

        /// <summary>
        /// 騎兵目標選擇：優先盾兵和弓兵
        /// </summary>
        private static BattleSoldier SelectCavalryTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            // 優先選擇盾兵
            var shieldmen = enemies.Where(e => e.Type == SoldierType.Shieldman).ToList();
            if (shieldmen.Count > 0)
            {
                return SelectNearestTarget(soldier, shieldmen);
            }

            // 其次選擇弓兵
            var archers = enemies.Where(e => e.Type == SoldierType.Archer).ToList();
            if (archers.Count > 0)
            {
                return SelectNearestTarget(soldier, archers);
            }

            return SelectNearestTarget(soldier, enemies);
        }

        /// <summary>
        /// 弓兵目標選擇：優先血量最低的敵人
        /// </summary>
        private static BattleSoldier SelectArcherTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            // 選擇血量百分比最低的敵人
            return enemies
                .OrderBy(e => e.CurrentHp / e.MaxHp)
                .ThenBy(e => soldier.GetDistanceTo(e.Position))
                .FirstOrDefault();
        }

        /// <summary>
        /// 選擇最近的目標
        /// </summary>
        private static BattleSoldier SelectNearestTarget(BattleSoldier soldier, List<BattleSoldier> enemies)
        {
            return enemies
                .OrderBy(e => soldier.GetDistanceTo(e.Position))
                .FirstOrDefault();
        }

        #endregion

        #region 移動計算

        /// <summary>
        /// 計算前進方向（根據陣營）
        /// </summary>
        public static Vector2 GetAdvanceDirection(BattleSoldier soldier, Battlefield battlefield)
        {
            var faction = battlefield.GetFaction(soldier.NationId);
            if (faction == null)
                return Vector2.zero;

            // 向戰場中心前進
            Vector2 targetPos = battlefield.Center;

            // 攻方向右前進，守方向左前進
            if (faction.Role == FactionRole.Attacker)
            {
                targetPos = new Vector2(battlefield.BattlefieldSize.x * 0.7f, battlefield.BattlefieldSize.y / 2);
            }
            else if (faction.Role == FactionRole.Defender)
            {
                targetPos = new Vector2(battlefield.BattlefieldSize.x * 0.3f, battlefield.BattlefieldSize.y / 2);
            }
            else // Third party
            {
                targetPos = battlefield.Center;
            }

            return (targetPos - soldier.Position).normalized;
        }

        /// <summary>
        /// 計算撤退方向
        /// </summary>
        public static Vector2 GetRetreatDirection(BattleSoldier soldier, Battlefield battlefield)
        {
            var faction = battlefield.GetFaction(soldier.NationId);
            if (faction == null)
                return Vector2.zero;

            return (faction.RetreatPoint - soldier.Position).normalized;
        }

        /// <summary>
        /// 計算追擊方向
        /// </summary>
        public static Vector2 GetChaseDirection(BattleSoldier soldier, BattleSoldier target)
        {
            if (target == null || !target.IsAlive)
                return Vector2.zero;

            return (target.Position - soldier.Position).normalized;
        }

        /// <summary>
        /// 弓兵保持距離計算
        /// </summary>
        public static Vector2 GetArcherKiteDirection(BattleSoldier archer, BattleSoldier nearestEnemy)
        {
            if (nearestEnemy == null)
                return Vector2.zero;

            float distance = archer.GetDistanceTo(nearestEnemy.Position);
            float preferredRange = archer.AttackRange * 0.8f; // 維持在攻擊範圍的 80%

            if (distance < preferredRange)
            {
                // 太近了，後退
                return (archer.Position - nearestEnemy.Position).normalized;
            }

            return Vector2.zero; // 距離合適，不需要移動
        }

        #endregion

        #region 戰場判定

        /// <summary>
        /// 檢查士兵是否到達撤退點
        /// </summary>
        public static bool HasReachedRetreatPoint(BattleSoldier soldier, Battlefield battlefield)
        {
            var faction = battlefield.GetFaction(soldier.NationId);
            if (faction == null)
                return false;

            return soldier.GetDistanceTo(faction.RetreatPoint) < 20f;
        }

        /// <summary>
        /// 檢查士兵是否到達敵方區域（可以攻城）
        /// </summary>
        public static bool IsInSiegeRange(BattleSoldier soldier, Battlefield battlefield)
        {
            var faction = battlefield.GetFaction(soldier.NationId);
            if (faction == null || faction.Role != FactionRole.Attacker)
                return false;

            // 攻方士兵到達右側城牆區域
            return soldier.Position.x > battlefield.BattlefieldSize.x * 0.85f;
        }

        /// <summary>
        /// 檢查位置是否在戰場範圍內
        /// </summary>
        public static bool IsInBattlefield(Vector2 position, Battlefield battlefield)
        {
            return position.x >= 0 && position.x <= battlefield.BattlefieldSize.x &&
                   position.y >= 0 && position.y <= battlefield.BattlefieldSize.y;
        }

        /// <summary>
        /// 限制位置在戰場範圍內
        /// </summary>
        public static Vector2 ClampToBattlefield(Vector2 position, Battlefield battlefield)
        {
            return new Vector2(
                Mathf.Clamp(position.x, 0, battlefield.BattlefieldSize.x),
                Mathf.Clamp(position.y, 0, battlefield.BattlefieldSize.y)
            );
        }

        #endregion

        #region 統計計算

        /// <summary>
        /// 計算國家戰力評估
        /// </summary>
        public static int CalculateNationPower(List<BattleSoldier> soldiers)
        {
            int power = 0;

            foreach (var soldier in soldiers)
            {
                if (!soldier.IsAlive) continue;

                // 基礎戰力 = 攻擊 + 防禦 + (血量/10)
                int soldierPower = Mathf.RoundToInt(
                    soldier.Attack + soldier.Defense + (soldier.CurrentHp / 10f)
                );

                power += soldierPower;
            }

            return power;
        }

        /// <summary>
        /// 計算預估勝率
        /// </summary>
        public static float CalculateWinRate(int myPower, int enemyPower)
        {
            if (myPower + enemyPower == 0)
                return 0.5f;

            return (float)myPower / (myPower + enemyPower);
        }

        #endregion
    }
}

