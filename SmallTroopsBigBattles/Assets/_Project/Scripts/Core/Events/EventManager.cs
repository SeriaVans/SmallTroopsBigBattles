using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Events
{
    /// <summary>
    /// 事件介面
    /// </summary>
    public interface IGameEvent { }

    /// <summary>
    /// 事件管理器 - 發布/訂閱模式
    /// </summary>
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 訂閱事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            _eventHandlers[eventType].Add(handler);
        }

        /// <summary>
        /// 取消訂閱
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// 發布事件
        /// </summary>
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType].ToArray())
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(gameEvent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventManager] Error handling event {eventType.Name}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void Clear()
        {
            _eventHandlers.Clear();
        }
    }

    #region 遊戲事件定義

    public class ResourceChangedEvent : IGameEvent
    {
        public ResourceType Type;
        public int OldValue;
        public int NewValue;
        public int Delta;
    }

    public class BuildingConstructedEvent : IGameEvent
    {
        public int TerritoryId;
        public Data.BuildingData Building;
    }

    public class SoldierTrainedEvent : IGameEvent
    {
        public SoldierType SoldierType;
        public int Count;
    }

    public class GeneralObtainedEvent : IGameEvent
    {
        public Data.GeneralData General;
    }

    public class BattleStartedEvent : IGameEvent
    {
        public int BattleId;
        public int AttackerNationId;
        public int DefenderNationId;
    }

    public class BattleEndedEvent : IGameEvent
    {
        public int BattleId;
        public int WinnerNationId;
    }

    public class CityConqueredEvent : IGameEvent
    {
        public int CityId;
        public int NewOwnerId;
    }

    public class PhaseChangedEvent : IGameEvent
    {
        public GamePhase OldPhase;
        public GamePhase NewPhase;
    }

    #endregion
}
