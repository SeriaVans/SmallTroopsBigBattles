using System;
using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Core.Managers
{
    /// <summary>
    /// 軍隊管理器 - 處理士兵訓練與編制
    /// </summary>
    public class ArmyManager : Singleton<ArmyManager>
    {
        // 訓練佇列
        private Queue<TrainingTask> _trainingQueue = new Queue<TrainingTask>();
        private TrainingTask _currentTraining;

        // 事件
        public event Action<SoldierType, int> OnSoldierTrained;
        public event Action<TrainingTask> OnTrainingStarted;
        public event Action<TrainingTask> OnTrainingCompleted;

        protected override void OnSingletonAwake()
        {
            // 初始化
        }

        private void Update()
        {
            ProcessTraining();
        }

        /// <summary>
        /// 處理訓練佇列
        /// </summary>
        private void ProcessTraining()
        {
            if (_currentTraining == null && _trainingQueue.Count > 0)
            {
                _currentTraining = _trainingQueue.Dequeue();
                _currentTraining.StartTime = DateTime.Now;
                OnTrainingStarted?.Invoke(_currentTraining);
            }

            if (_currentTraining != null)
            {
                if (DateTime.Now >= _currentTraining.EndTime)
                {
                    CompleteTraining();
                }
            }
        }

        private void CompleteTraining()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            var task = _currentTraining;
            int actualCount = Mathf.Min(task.Count,
                PlayerArmy.MaxSoldiers - player.Army.TotalSoldiers);

            if (actualCount > 0)
            {
                int currentCount = player.Army.GetSoldierCount(task.SoldierType);
                player.Army.SetSoldierCount(task.SoldierType, currentCount + actualCount);

                Debug.Log($"[ArmyManager] 訓練完成: {task.SoldierType} x{actualCount}");

                OnSoldierTrained?.Invoke(task.SoldierType, actualCount);
                OnTrainingCompleted?.Invoke(task);

                EventManager.Instance.Publish(new SoldierTrainedEvent
                {
                    SoldierType = task.SoldierType,
                    Count = actualCount
                });
            }

            _currentTraining = null;
        }

        /// <summary>
        /// 開始訓練士兵
        /// </summary>
        public bool StartTraining(SoldierType type, int count)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            if (!player.Army.CanAddSoldiers(count))
            {
                Debug.LogWarning("[ArmyManager] 軍隊已達上限");
                return false;
            }

            // 檢查並消耗資源
            var costs = GetTrainingCost(type, count);
            if (!ResourceManager.Instance.ConsumeResources(costs))
            {
                Debug.LogWarning("[ArmyManager] 資源不足");
                return false;
            }

            var task = new TrainingTask
            {
                SoldierType = type,
                Count = count,
                Duration = GetTrainingTime(type, count)
            };

            _trainingQueue.Enqueue(task);
            Debug.Log($"[ArmyManager] 加入訓練佇列: {type} x{count}");

            return true;
        }

        /// <summary>
        /// 取得訓練費用
        /// </summary>
        public Dictionary<ResourceType, int> GetTrainingCost(SoldierType type, int count)
        {
            var baseCost = GetBaseSoldierCost(type);
            var result = new Dictionary<ResourceType, int>();

            foreach (var cost in baseCost)
            {
                result[cost.Key] = cost.Value * count;
            }

            return result;
        }

        private Dictionary<ResourceType, int> GetBaseSoldierCost(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Food, 10 },
                    { ResourceType.Wood, 5 },
                    { ResourceType.Copper, 5 }
                },
                SoldierType.Shieldman => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Food, 10 },
                    { ResourceType.Wood, 10 },
                    { ResourceType.Stone, 5 },
                    { ResourceType.Copper, 10 }
                },
                SoldierType.Cavalry => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Food, 20 },
                    { ResourceType.Wood, 10 },
                    { ResourceType.Copper, 20 }
                },
                SoldierType.Archer => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Food, 10 },
                    { ResourceType.Wood, 15 },
                    { ResourceType.Copper, 10 }
                },
                _ => new Dictionary<ResourceType, int>()
            };
        }

        /// <summary>
        /// 取得訓練時間 (秒)
        /// </summary>
        public int GetTrainingTime(SoldierType type, int count)
        {
            int baseTime = type switch
            {
                SoldierType.Spearman => 10,
                SoldierType.Shieldman => 15,
                SoldierType.Cavalry => 25,
                SoldierType.Archer => 12,
                _ => 10
            };

            return baseTime * count;
        }

        /// <summary>
        /// 取得當前訓練進度
        /// </summary>
        public float GetTrainingProgress()
        {
            if (_currentTraining == null) return 0;

            float elapsed = (float)(DateTime.Now - _currentTraining.StartTime).TotalSeconds;
            return Mathf.Clamp01(elapsed / _currentTraining.Duration);
        }

        /// <summary>
        /// 取得訓練剩餘時間
        /// </summary>
        public float GetTrainingRemainingTime()
        {
            if (_currentTraining == null) return 0;
            return Mathf.Max(0, (float)(_currentTraining.EndTime - DateTime.Now).TotalSeconds);
        }

        /// <summary>
        /// 取得訓練佇列長度
        /// </summary>
        public int GetQueueLength()
        {
            return _trainingQueue.Count + (_currentTraining != null ? 1 : 0);
        }

        /// <summary>
        /// 取得士兵數量
        /// </summary>
        public int GetSoldierCount(SoldierType type)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            return player?.Army.GetSoldierCount(type) ?? 0;
        }

        /// <summary>
        /// 取得總士兵數
        /// </summary>
        public int GetTotalSoldiers()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            return player?.Army.TotalSoldiers ?? 0;
        }

        /// <summary>
        /// 取得士兵上限
        /// </summary>
        public int GetMaxSoldiers()
        {
            return PlayerArmy.MaxSoldiers;
        }

        /// <summary>
        /// 消耗士兵 (戰鬥損失)
        /// </summary>
        public void ConsumeSoldiers(SoldierType type, int count)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            int current = player.Army.GetSoldierCount(type);
            player.Army.SetSoldierCount(type, current - count);
        }
    }

    /// <summary>
    /// 訓練任務
    /// </summary>
    [Serializable]
    public class TrainingTask
    {
        public SoldierType SoldierType;
        public int Count;
        public int Duration; // 秒
        public DateTime StartTime;
        public DateTime EndTime => StartTime.AddSeconds(Duration);
    }
}
