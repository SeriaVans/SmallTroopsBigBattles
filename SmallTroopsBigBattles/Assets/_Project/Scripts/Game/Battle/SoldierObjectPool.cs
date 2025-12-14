using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 士兵對象池 - 用於遊戲對象的複用，優化性能
    /// </summary>
    public class SoldierObjectPool : MonoBehaviour
    {
        [Header("對象池設定")]
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private int initialPoolSize = 100;
        [SerializeField] private int maxPoolSize = 500;
        [SerializeField] private Transform poolContainer;

        /// <summary>可用對象隊列</summary>
        private Queue<GameObject> _availableObjects = new Queue<GameObject>();

        /// <summary>所有已創建的對象</summary>
        private List<GameObject> _allObjects = new List<GameObject>();

        /// <summary>活躍對象數量</summary>
        public int ActiveCount => _allObjects.Count - _availableObjects.Count;

        /// <summary>池中可用數量</summary>
        public int AvailableCount => _availableObjects.Count;

        /// <summary>總數量</summary>
        public int TotalCount => _allObjects.Count;

        private void Awake()
        {
            // 創建容器
            if (poolContainer == null)
            {
                var containerObj = new GameObject("SoldierPoolContainer");
                containerObj.transform.SetParent(transform);
                poolContainer = containerObj.transform;
            }

            // 預先創建對象
            PrewarmPool();
        }

        /// <summary>
        /// 預熱對象池
        /// </summary>
        public void PrewarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewObject();
            }

            Debug.Log($"[SoldierObjectPool] 預熱完成，創建 {initialPoolSize} 個對象");
        }

        /// <summary>
        /// 創建新對象
        /// </summary>
        private GameObject CreateNewObject()
        {
            if (soldierPrefab == null)
            {
                Debug.LogError("[SoldierObjectPool] 缺少士兵預製體！");
                return null;
            }

            var obj = Instantiate(soldierPrefab, poolContainer);
            obj.SetActive(false);
            obj.name = $"PooledSoldier_{_allObjects.Count}";

            _allObjects.Add(obj);
            _availableObjects.Enqueue(obj);

            return obj;
        }

        /// <summary>
        /// 從池中獲取對象
        /// </summary>
        public GameObject Get()
        {
            GameObject obj;

            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            else if (_allObjects.Count < maxPoolSize)
            {
                obj = CreateNewObject();
                if (obj != null)
                {
                    _availableObjects.Dequeue(); // 移除剛加入的
                }
            }
            else
            {
                Debug.LogWarning($"[SoldierObjectPool] 對象池已滿（{maxPoolSize}），無法獲取新對象");
                return null;
            }

            if (obj != null)
            {
                obj.SetActive(true);
            }

            return obj;
        }

        /// <summary>
        /// 從池中獲取對象並設置位置
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        /// <summary>
        /// 歸還對象到池中
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            _availableObjects.Enqueue(obj);
        }

        /// <summary>
        /// 歸還所有活躍對象
        /// </summary>
        public void ReturnAll()
        {
            foreach (var obj in _allObjects)
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                    _availableObjects.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// 清空對象池
        /// </summary>
        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            _allObjects.Clear();
            _availableObjects.Clear();
        }

        /// <summary>
        /// 設置預製體
        /// </summary>
        public void SetPrefab(GameObject prefab)
        {
            soldierPrefab = prefab;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }

    /// <summary>
    /// 泛型對象池
    /// </summary>
    /// <typeparam name="T">對象類型</typeparam>
    public class GenericObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _container;
        private readonly Queue<T> _availableObjects = new Queue<T>();
        private readonly List<T> _allObjects = new List<T>();
        private readonly int _maxSize;

        public int ActiveCount => _allObjects.Count - _availableObjects.Count;
        public int AvailableCount => _availableObjects.Count;
        public int TotalCount => _allObjects.Count;

        public GenericObjectPool(T prefab, Transform container, int initialSize = 50, int maxSize = 500)
        {
            _prefab = prefab;
            _container = container;
            _maxSize = maxSize;

            // 預熱
            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        private T CreateNew()
        {
            var obj = Object.Instantiate(_prefab, _container);
            obj.gameObject.SetActive(false);
            obj.name = $"Pooled_{typeof(T).Name}_{_allObjects.Count}";

            _allObjects.Add(obj);
            _availableObjects.Enqueue(obj);

            return obj;
        }

        public T Get()
        {
            T obj;

            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            else if (_allObjects.Count < _maxSize)
            {
                obj = CreateNew();
                _availableObjects.Dequeue();
            }
            else
            {
                return null;
            }

            obj?.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_container);
            _availableObjects.Enqueue(obj);
        }

        public void ReturnAll()
        {
            foreach (var obj in _allObjects)
            {
                if (obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(false);
                    _availableObjects.Enqueue(obj);
                }
            }
        }

        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _allObjects.Clear();
            _availableObjects.Clear();
        }
    }
}

