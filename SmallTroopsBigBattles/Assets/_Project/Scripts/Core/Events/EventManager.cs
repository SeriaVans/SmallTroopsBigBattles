using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Events
{
    /// <summary>
    /// 事件管理器 - 負責遊戲內事件的訂閱和發布
    /// </summary>
    public class EventManager : SingletonBase<EventManager>
    {
        // 事件訂閱者字典：事件類型 -> 訂閱者列表
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        protected override void OnSingletonAwake()
        {
            Debug.Log("[EventManager] 事件系統初始化完成");
        }

        /// <summary>
        /// 訂閱事件
        /// </summary>
        /// <typeparam name="T">事件類型</typeparam>
        /// <param name="handler">事件處理方法</param>
        public void Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            var eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            if (!_subscribers[eventType].Contains(handler))
            {
                _subscribers[eventType].Add(handler);
            }
        }

        /// <summary>
        /// 取消訂閱事件
        /// </summary>
        /// <typeparam name="T">事件類型</typeparam>
        /// <param name="handler">事件處理方法</param>
        public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            var eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// 發布事件
        /// </summary>
        /// <typeparam name="T">事件類型</typeparam>
        /// <param name="gameEvent">事件實例</param>
        public void Publish<T>(T gameEvent) where T : GameEvent
        {
            var eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                return;
            }

            // 複製列表以避免在遍歷時修改
            var handlers = new List<Delegate>(_subscribers[eventType]);

            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(gameEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventManager] 執行事件處理器時發生錯誤: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// 清除所有訂閱
        /// </summary>
        public void ClearAllSubscribers()
        {
            _subscribers.Clear();
            Debug.Log("[EventManager] 已清除所有事件訂閱");
        }

        /// <summary>
        /// 清除特定事件的所有訂閱
        /// </summary>
        /// <typeparam name="T">事件類型</typeparam>
        public void ClearSubscribers<T>() where T : GameEvent
        {
            var eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Clear();
            }
        }

        /// <summary>
        /// 獲取特定事件的訂閱者數量
        /// </summary>
        /// <typeparam name="T">事件類型</typeparam>
        /// <returns>訂閱者數量</returns>
        public int GetSubscriberCount<T>() where T : GameEvent
        {
            var eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                return _subscribers[eventType].Count;
            }

            return 0;
        }

        protected override void OnDestroy()
        {
            ClearAllSubscribers();
            base.OnDestroy();
        }
    }
}

