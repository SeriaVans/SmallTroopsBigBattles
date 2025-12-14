using System;
using UnityEngine;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Battle
{
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
        Idle,       // 閒置
        Advancing,  // 前進中 (初始狀態)
        Moving,     // 移動中 (追擊目標)
        Attacking,  // 攻擊中
        Retreating, // 撤退中
        Dead        // 死亡
    }

    /// <summary>
    /// 士兵基礎屬性配置
    /// </summary>
    [Serializable]
    public class SoldierStats
    {
        public float MaxHp;
        public float Attack;
        public float Defense;
        public float Speed;
        public float AttackRange;
        public float AttackSpeed;   // 攻擊間隔（秒）
        public float SiegeDamage;   // 攻城係數

        /// <summary>
        /// 根據士兵類型獲取基礎屬性
        /// </summary>
        public static SoldierStats GetBaseStats(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => new SoldierStats
                {
                    MaxHp = 100f,
                    Attack = 25f,
                    Defense = 15f,
                    Speed = 3f,
                    AttackRange = 1.5f,
                    AttackSpeed = 1.2f,
                    SiegeDamage = 0.5f
                },
                SoldierType.Shieldman => new SoldierStats
                {
                    MaxHp = 150f,
                    Attack = 15f,
                    Defense = 30f,
                    Speed = 2.5f,
                    AttackRange = 1.2f,
                    AttackSpeed = 1.5f,
                    SiegeDamage = 0.3f
                },
                SoldierType.Cavalry => new SoldierStats
                {
                    MaxHp = 120f,
                    Attack = 30f,
                    Defense = 10f,
                    Speed = 5f,
                    AttackRange = 1.8f,
                    AttackSpeed = 1.0f,
                    SiegeDamage = 0.3f
                },
                SoldierType.Archer => new SoldierStats
                {
                    MaxHp = 60f,
                    Attack = 35f,
                    Defense = 5f,
                    Speed = 3f,
                    AttackRange = 8f,
                    AttackSpeed = 1.8f,
                    SiegeDamage = 0.8f
                },
                _ => new SoldierStats()
            };
        }
    }

    /// <summary>
    /// 戰場上的士兵單位
    /// </summary>
    [Serializable]
    public class BattleSoldier
    {
        /// <summary>士兵唯一 ID</summary>
        public long SoldierId;

        /// <summary>所屬玩家 ID</summary>
        public string OwnerId;

        /// <summary>所屬國家 ID</summary>
        public string NationId;

        /// <summary>兵種類型</summary>
        public SoldierType Type;

        /// <summary>當前狀態</summary>
        public SoldierState State;

        // 戰鬥屬性
        public float MaxHp;
        public float CurrentHp;
        public float Attack;
        public float Defense;
        public float Speed;
        public float AttackRange;
        public float AttackSpeed;
        public float SiegeDamage;

        // 位置與目標
        public Vector2 Position;
        public Vector2 TargetPosition;
        public long TargetId;

        // 計時器
        public float AttackCooldown;
        public float LastAttackTime;

        /// <summary>是否存活</summary>
        public bool IsAlive => State != SoldierState.Dead && CurrentHp > 0;

        /// <summary>是否可以攻擊</summary>
        public bool CanAttack => AttackCooldown <= 0 && State != SoldierState.Retreating && State != SoldierState.Dead;

        /// <summary>
        /// 建構函式
        /// </summary>
        public BattleSoldier()
        {
            SoldierId = GenerateId();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public BattleSoldier(SoldierType type, string ownerId, string nationId, Vector2 spawnPos)
        {
            SoldierId = GenerateId();
            Type = type;
            OwnerId = ownerId;
            NationId = nationId;
            Position = spawnPos;
            State = SoldierState.Advancing;

            // 應用基礎屬性
            var stats = SoldierStats.GetBaseStats(type);
            MaxHp = stats.MaxHp;
            CurrentHp = stats.MaxHp;
            Attack = stats.Attack;
            Defense = stats.Defense;
            Speed = stats.Speed;
            AttackRange = stats.AttackRange;
            AttackSpeed = stats.AttackSpeed;
            SiegeDamage = stats.SiegeDamage;
        }

        /// <summary>
        /// 生成唯一 ID
        /// </summary>
        private static long GenerateId()
        {
            return DateTime.Now.Ticks + UnityEngine.Random.Range(0, 10000);
        }

        /// <summary>
        /// 受到傷害
        /// </summary>
        public float TakeDamage(float damage, SoldierType attackerType)
        {
            // 計算克制係數
            float multiplier = GetDamageMultiplier(attackerType, Type);

            // 計算實際傷害（考慮防禦）
            float actualDamage = damage * multiplier * (100f / (100f + Defense));

            // 隨機浮動 (0.9 ~ 1.1)
            actualDamage *= UnityEngine.Random.Range(0.9f, 1.1f);

            // 撤退時受到額外傷害
            if (State == SoldierState.Retreating)
            {
                actualDamage *= 1.5f;
            }

            CurrentHp -= actualDamage;

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                State = SoldierState.Dead;
            }

            return actualDamage;
        }

        /// <summary>
        /// 獲取克制傷害倍率
        /// </summary>
        public static float GetDamageMultiplier(SoldierType attacker, SoldierType defender)
        {
            // 槍兵克制騎兵
            if (attacker == SoldierType.Spearman && defender == SoldierType.Cavalry)
                return 1.5f;

            // 騎兵克制盾兵
            if (attacker == SoldierType.Cavalry && defender == SoldierType.Shieldman)
                return 1.5f;

            // 弓兵被所有人克制
            if (defender == SoldierType.Archer)
                return 1.5f;

            // 盾兵抗弓兵傷害
            if (attacker == SoldierType.Archer && defender == SoldierType.Shieldman)
                return 0.5f;

            return 1.0f;
        }

        /// <summary>
        /// 設置目標
        /// </summary>
        public void SetTarget(BattleSoldier target)
        {
            if (target != null && target.IsAlive)
            {
                TargetId = target.SoldierId;
                TargetPosition = target.Position;
                State = SoldierState.Moving;
            }
            else
            {
                TargetId = 0;
                State = SoldierState.Advancing;
            }
        }

        /// <summary>
        /// 清除目標
        /// </summary>
        public void ClearTarget()
        {
            TargetId = 0;
            State = SoldierState.Advancing;
        }

        /// <summary>
        /// 開始撤退
        /// </summary>
        public void StartRetreat()
        {
            State = SoldierState.Retreating;
            TargetId = 0;
        }

        /// <summary>
        /// 更新攻擊冷卻
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (AttackCooldown > 0)
            {
                AttackCooldown -= deltaTime;
            }
        }

        /// <summary>
        /// 重置攻擊冷卻
        /// </summary>
        public void ResetAttackCooldown()
        {
            AttackCooldown = AttackSpeed;
            LastAttackTime = Time.time;
        }

        /// <summary>
        /// 移動
        /// </summary>
        public void Move(Vector2 direction, float deltaTime)
        {
            Position += direction.normalized * Speed * deltaTime;
        }

        /// <summary>
        /// 獲取到目標的距離
        /// </summary>
        public float GetDistanceTo(Vector2 targetPos)
        {
            return Vector2.Distance(Position, targetPos);
        }

        /// <summary>
        /// 是否在攻擊範圍內
        /// </summary>
        public bool IsInAttackRange(Vector2 targetPos)
        {
            return GetDistanceTo(targetPos) <= AttackRange;
        }
    }

    /// <summary>
    /// 士兵快照（用於網路同步）
    /// </summary>
    [Serializable]
    public class SoldierSnapshot
    {
        public long SoldierId;
        public Vector2 Position;
        public float HpPercent;
        public SoldierState State;
        public long TargetId;

        public SoldierSnapshot() { }

        public SoldierSnapshot(BattleSoldier soldier)
        {
            SoldierId = soldier.SoldierId;
            Position = soldier.Position;
            HpPercent = soldier.CurrentHp / soldier.MaxHp;
            State = soldier.State;
            TargetId = soldier.TargetId;
        }
    }

    /// <summary>
    /// 等待中的士兵（在士兵池中）
    /// </summary>
    [Serializable]
    public class PooledSoldier
    {
        public string PlayerId;
        public SoldierType Type;
        public DateTime JoinTime;

        public PooledSoldier(string playerId, SoldierType type)
        {
            PlayerId = playerId;
            Type = type;
            JoinTime = DateTime.Now;
        }
    }
}

