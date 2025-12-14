using UnityEngine;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// 泛型單例基類 - 用於 MonoBehaviour 類型的單例
    /// </summary>
    /// <typeparam name="T">繼承此類的類型</typeparam>
    public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// 獲取單例實例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 嘗試在場景中找到現有實例
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            // 創建新的 GameObject 並添加組件
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[{typeof(T).Name}]";

                            // 確保不會在場景切換時被銷毀
                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[Singleton] 創建新的 {typeof(T).Name} 實例");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// 檢查實例是否存在
        /// </summary>
        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] 發現重複的 {typeof(T).Name} 實例，銷毀重複物件");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 單例初始化時調用 - 子類可覆寫
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }

    /// <summary>
    /// 純 C# 單例基類 - 用於非 MonoBehaviour 類型
    /// </summary>
    /// <typeparam name="T">繼承此類的類型</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                        _instance.OnInitialize();
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null;

        /// <summary>
        /// 初始化時調用 - 子類可覆寫
        /// </summary>
        protected virtual void OnInitialize()
        {
        }
    }
}

