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
        private Queue<TrainingTask> _trainingQueue = new Queue<TrainingTask>();
        private TrainingTask _currentTraining;

        public event Action<SoldierType, int> OnSoldierTrained;
        public event Action<TrainingTask> OnTrainingStarted;
        public event Action<TrainingTask> OnTrainingCompleted;

        private void Update()
        {
            ProcessTraining();
        }

        private void ProcessTraining()
        {
            if (_currentTraining == null && _trainingQueue.Count > 0)
            {
                _currentTraining = _trainingQueue.Dequeue();
                _currentTraining.StartTime = DateTime.Now;
                OnTrainingStarted?.Invoke(_currentTraining);
            }

            if (_currentTraining != null && DateTime.Now >= _currentTraining.EndTime)
            {
                CompleteTraining();
            }
        }

        private void CompleteTraining()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            var task = _currentTraining;
            int actualCount = Mathf.Min(task.Count, PlayerArmy.MaxSoldiers - player.Army.TotalSoldiers);

            if (actualCount > 0)
            {
                int current = player.Army.GetSoldierCount(task.SoldierType);
                player.Army.SetSoldierCount(task.SoldierType, current + actualCount);

                OnSoldierTrained?.Invoke(task.SoldierType, actualCount);
                OnTrainingCompleted?.Invoke(task);
                EventManager.Instance.Publish(new SoldierTrainedEvent { SoldierType = task.SoldierType, Count = actualCount });
            }

            _currentTraining = null;
        }

        public bool StartTraining(SoldierType type, int count)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null || !player.Army.CanAddSoldiers(count)) return false;

            var costs = GetTrainingCost(type, count);
            if (!ResourceManager.Instance.ConsumeResources(costs)) return false;

            _trainingQueue.Enqueue(new TrainingTask
            {
                SoldierType = type,
                Count = count,
                Duration = GetTrainingTime(type, count)
            });
            return true;
        }

        public Dictionary<ResourceType, int> GetTrainingCost(SoldierType type, int count)
        {
            var baseCost = GetBaseSoldierCost(type);
            var result = new Dictionary<ResourceType, int>();
            foreach (var cost in baseCost) result[cost.Key] = cost.Value * count;
            return result;
        }

        private Dictionary<ResourceType, int> GetBaseSoldierCost(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => new Dictionary<ResourceType, int> { { ResourceType.Food, 10 }, { ResourceType.Wood, 5 }, { ResourceType.Copper, 5 } },
                SoldierType.Shieldman => new Dictionary<ResourceType, int> { { ResourceType.Food, 10 }, { ResourceType.Wood, 10 }, { ResourceType.Stone, 5 }, { ResourceType.Copper, 10 } },
                SoldierType.Cavalry => new Dictionary<ResourceType, int> { { ResourceType.Food, 20 }, { ResourceType.Wood, 10 }, { ResourceType.Copper, 20 } },
                SoldierType.Archer => new Dictionary<ResourceType, int> { { ResourceType.Food, 10 }, { ResourceType.Wood, 15 }, { ResourceType.Copper, 10 } },
                _ => new Dictionary<ResourceType, int>()
            };
        }

        public int GetTrainingTime(SoldierType type, int count)
        {
            int baseTime = type switch { SoldierType.Spearman => 10, SoldierType.Shieldman => 15, SoldierType.Cavalry => 25, SoldierType.Archer => 12, _ => 10 };
            return baseTime * count;
        }

        public float GetTrainingProgress()
        {
            if (_currentTraining == null) return 0;
            float elapsed = (float)(DateTime.Now - _currentTraining.StartTime).TotalSeconds;
            return Mathf.Clamp01(elapsed / _currentTraining.Duration);
        }

        public float GetTrainingRemainingTime()
        {
            if (_currentTraining == null) return 0;
            return Mathf.Max(0, (float)(_currentTraining.EndTime - DateTime.Now).TotalSeconds);
        }

        public int GetQueueLength() => _trainingQueue.Count + (_currentTraining != null ? 1 : 0);
        public int GetSoldierCount(SoldierType type) => GameManager.Instance?.CurrentPlayer?.Army.GetSoldierCount(type) ?? 0;
        public int GetTotalSoldiers() => GameManager.Instance?.CurrentPlayer?.Army.TotalSoldiers ?? 0;
        public int GetMaxSoldiers() => PlayerArmy.MaxSoldiers;

        public void ConsumeSoldiers(SoldierType type, int count)
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;
            int current = player.Army.GetSoldierCount(type);
            player.Army.SetSoldierCount(type, current - count);
        }
    }

    [Serializable]
    public class TrainingTask
    {
        public SoldierType SoldierType;
        public int Count;
        public int Duration;
        public DateTime StartTime;
        public DateTime EndTime => StartTime.AddSeconds(Duration);
    }
}
