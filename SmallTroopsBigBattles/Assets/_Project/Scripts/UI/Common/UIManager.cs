using System;
using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// UI 管理器 - 控制所有 UI 面板的顯示與隱藏
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("UI 層級")]
        [SerializeField] private Transform _normalLayer;    // 一般面板層
        [SerializeField] private Transform _popupLayer;     // 彈窗層
        [SerializeField] private Transform _topLayer;       // 最上層 (系統訊息等)

        [Header("面板預製體")]
        [SerializeField] private List<BasePanel> _panelPrefabs = new List<BasePanel>();

        // 已開啟的面板
        private Dictionary<Type, BasePanel> _openPanels = new Dictionary<Type, BasePanel>();

        // 面板預製體快取
        private Dictionary<Type, BasePanel> _prefabCache = new Dictionary<Type, BasePanel>();

        // 面板歷史堆疊 (用於返回功能)
        private Stack<BasePanel> _panelStack = new Stack<BasePanel>();

        public event Action<BasePanel> OnPanelOpened;
        public event Action<BasePanel> OnPanelClosed;

        protected override void OnSingletonAwake()
        {
            InitializePrefabCache();
            CreateUILayers();
        }

        private void InitializePrefabCache()
        {
            foreach (var prefab in _panelPrefabs)
            {
                if (prefab != null)
                {
                    _prefabCache[prefab.GetType()] = prefab;
                }
            }
        }

        private void CreateUILayers()
        {
            // 如果沒有設定層級，自動建立
            if (_normalLayer == null)
            {
                _normalLayer = CreateLayer("NormalLayer", 0);
            }
            if (_popupLayer == null)
            {
                _popupLayer = CreateLayer("PopupLayer", 100);
            }
            if (_topLayer == null)
            {
                _topLayer = CreateLayer("TopLayer", 200);
            }
        }

        private Transform CreateLayer(string name, int sortOrder)
        {
            var layerObj = new GameObject(name);
            layerObj.transform.SetParent(transform);

            var canvas = layerObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            var scaler = layerObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            layerObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return layerObj.transform;
        }

        /// <summary>
        /// 開啟面板
        /// </summary>
        public T OpenPanel<T>(bool addToStack = true) where T : BasePanel
        {
            var panelType = typeof(T);

            // 如果已開啟，直接返回
            if (_openPanels.TryGetValue(panelType, out var existingPanel))
            {
                existingPanel.transform.SetAsLastSibling();
                return existingPanel as T;
            }

            // 取得預製體
            if (!_prefabCache.TryGetValue(panelType, out var prefab))
            {
                Debug.LogError($"[UIManager] 找不到面板預製體: {panelType.Name}");
                return null;
            }

            // 決定層級
            Transform parent = GetLayerByPanelType(prefab.PanelLayer);

            // 實例化
            var panel = Instantiate(prefab, parent) as T;
            panel.name = panelType.Name;

            _openPanels[panelType] = panel;

            if (addToStack)
            {
                _panelStack.Push(panel);
            }

            panel.OnOpen();
            OnPanelOpened?.Invoke(panel);

            Debug.Log($"[UIManager] 開啟面板: {panelType.Name}");
            return panel;
        }

        /// <summary>
        /// 關閉面板
        /// </summary>
        public void ClosePanel<T>() where T : BasePanel
        {
            ClosePanel(typeof(T));
        }

        public void ClosePanel(Type panelType)
        {
            if (!_openPanels.TryGetValue(panelType, out var panel))
            {
                return;
            }

            panel.OnClose();
            _openPanels.Remove(panelType);

            OnPanelClosed?.Invoke(panel);
            Destroy(panel.gameObject);

            Debug.Log($"[UIManager] 關閉面板: {panelType.Name}");
        }

        /// <summary>
        /// 關閉所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            var panelsToClose = new List<Type>(_openPanels.Keys);
            foreach (var panelType in panelsToClose)
            {
                ClosePanel(panelType);
            }
            _panelStack.Clear();
        }

        /// <summary>
        /// 返回上一個面板
        /// </summary>
        public void Back()
        {
            if (_panelStack.Count > 1)
            {
                var currentPanel = _panelStack.Pop();
                ClosePanel(currentPanel.GetType());
            }
        }

        /// <summary>
        /// 取得已開啟的面板
        /// </summary>
        public T GetPanel<T>() where T : BasePanel
        {
            if (_openPanels.TryGetValue(typeof(T), out var panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 檢查面板是否開啟
        /// </summary>
        public bool IsPanelOpen<T>() where T : BasePanel
        {
            return _openPanels.ContainsKey(typeof(T));
        }

        private Transform GetLayerByPanelType(PanelLayer layer)
        {
            return layer switch
            {
                PanelLayer.Normal => _normalLayer,
                PanelLayer.Popup => _popupLayer,
                PanelLayer.Top => _topLayer,
                _ => _normalLayer
            };
        }

        /// <summary>
        /// 註冊面板預製體 (動態加入)
        /// </summary>
        public void RegisterPanelPrefab(BasePanel prefab)
        {
            if (prefab != null)
            {
                _prefabCache[prefab.GetType()] = prefab;
            }
        }
    }

    /// <summary>
    /// UI 層級
    /// </summary>
    public enum PanelLayer
    {
        Normal,     // 一般面板
        Popup,      // 彈窗
        Top         // 最上層
    }
}
