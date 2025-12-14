using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Events
{
    /// <summary>
    /// 事件管理器 - 發布/訂閱模式
    /// </summary>
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<Type, Delegate> _eventHandlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 訂閱事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);

            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Combine(_eventHandlers[eventType], handler);
            }
            else
            {
                _eventHandlers[eventType] = handler;
            }
        }

        /// <summary>
        /// 取消訂閱事件
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);

            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Remove(_eventHandlers[eventType], handler);

                if (_eventHandlers[eventType] == null)
                {
                    _eventHandlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 發布事件
        /// </summary>
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var eventType = typeof(T);

            if (_eventHandlers.TryGetValue(eventType, out var handler))
            {
                (handler as Action<T>)?.Invoke(gameEvent);
            }
        }

        /// <summary>
        /// 清除所有事件訂閱
        /// </summary>
        public void ClearAll()
        {
            _eventHandlers.Clear();
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }
    }

    /// <summary>
    /// 遊戲事件介面
    /// </summary>
    public interface IGameEvent { }

    #region 遊戲事件定義

    // ===== 資源事件 =====
    public struct ResourceChangedEvent : IGameEvent
    {
        public ResourceType Type;
        public int OldValue;
        public int NewValue;
        public int Delta;
    }

    // ===== 建築事件 =====
    public struct BuildingConstructedEvent : IGameEvent
    {
        public int TerritoryId;
        public int BuildingSlot;
        public TerritoryBuildingType BuildingType;
    }

    public struct BuildingUpgradedEvent : IGameEvent
    {
        public int TerritoryId;
        public int BuildingSlot;
        public int NewLevel;
    }

    // ===== 士兵事件 =====
    public struct SoldierTrainedEvent : IGameEvent
    {
        public SoldierType Type;
        public int Count;
    }

    public struct ArmyChangedEvent : IGameEvent
    {
        public int TotalSoldiers;
    }

    // ===== 將領事件 =====
    public struct GeneralObtainedEvent : IGameEvent
    {
        public long GeneralId;
        public int Rarity;
    }

    public struct GeneralLevelUpEvent : IGameEvent
    {
        public long GeneralId;
        public int NewLevel;
    }

    // ===== 戰鬥事件 =====
    public struct BattleStartedEvent : IGameEvent
    {
        public int BattlefieldId;
        public int CityId;
    }

    public struct BattleEndedEvent : IGameEvent
    {
        public int BattlefieldId;
        public int WinnerNationId;
    }

    // ===== 城池事件 =====
    public struct CityConqueredEvent : IGameEvent
    {
        public int CityId;
        public int OldNationId;
        public int NewNationId;
    }

    // ===== UI 事件 =====
    public struct PanelOpenedEvent : IGameEvent
    {
        public string PanelName;
    }

    public struct PanelClosedEvent : IGameEvent
    {
        public string PanelName;
    }

    #endregion
}
