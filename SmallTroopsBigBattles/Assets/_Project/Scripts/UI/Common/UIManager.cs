using System;
using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core;

namespace SmallTroopsBigBattles.UI
{
    public enum PanelLayer { Normal, Popup, Top }

    public class UIManager : Singleton<UIManager>
    {
        [Header("UI 層級")]
        [SerializeField] private Transform _normalLayer;
        [SerializeField] private Transform _popupLayer;
        [SerializeField] private Transform _topLayer;

        [Header("面板預製體")]
        [SerializeField] private List<BasePanel> _panelPrefabs = new List<BasePanel>();

        private Dictionary<Type, BasePanel> _openPanels = new Dictionary<Type, BasePanel>();
        private Dictionary<Type, BasePanel> _prefabCache = new Dictionary<Type, BasePanel>();
        private Stack<BasePanel> _panelStack = new Stack<BasePanel>();

        public event Action<BasePanel> OnPanelOpened;
        public event Action<BasePanel> OnPanelClosed;

        protected override void OnSingletonAwake()
        {
            foreach (var prefab in _panelPrefabs)
                if (prefab != null) _prefabCache[prefab.GetType()] = prefab;
            CreateUILayers();
        }

        private void CreateUILayers()
        {
            if (_normalLayer == null) _normalLayer = CreateLayer("NormalLayer", 0);
            if (_popupLayer == null) _popupLayer = CreateLayer("PopupLayer", 100);
            if (_topLayer == null) _topLayer = CreateLayer("TopLayer", 200);
        }

        private Transform CreateLayer(string name, int sortOrder)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = obj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return obj.transform;
        }

        public T OpenPanel<T>(bool addToStack = true) where T : BasePanel
        {
            var type = typeof(T);
            if (_openPanels.TryGetValue(type, out var existing))
            {
                existing.transform.SetAsLastSibling();
                return existing as T;
            }
            if (!_prefabCache.TryGetValue(type, out var prefab)) return null;

            var panel = Instantiate(prefab, GetLayerByPanelType(prefab.PanelLayer)) as T;
            panel.name = type.Name;
            _openPanels[type] = panel;
            if (addToStack) _panelStack.Push(panel);
            panel.OnOpen();
            OnPanelOpened?.Invoke(panel);
            return panel;
        }

        public void ClosePanel<T>() where T : BasePanel => ClosePanel(typeof(T));

        public void ClosePanel(Type type)
        {
            if (!_openPanels.TryGetValue(type, out var panel)) return;
            panel.OnClose();
            _openPanels.Remove(type);
            OnPanelClosed?.Invoke(panel);
            Destroy(panel.gameObject);
        }

        public void CloseAllPanels()
        {
            foreach (var type in new List<Type>(_openPanels.Keys)) ClosePanel(type);
            _panelStack.Clear();
        }

        public void Back()
        {
            if (_panelStack.Count > 1)
            {
                var current = _panelStack.Pop();
                ClosePanel(current.GetType());
            }
        }

        public T GetPanel<T>() where T : BasePanel => _openPanels.TryGetValue(typeof(T), out var p) ? p as T : null;
        public bool IsPanelOpen<T>() where T : BasePanel => _openPanels.ContainsKey(typeof(T));

        private Transform GetLayerByPanelType(PanelLayer layer) => layer switch
        {
            PanelLayer.Normal => _normalLayer,
            PanelLayer.Popup => _popupLayer,
            PanelLayer.Top => _topLayer,
            _ => _normalLayer
        };

        public void RegisterPanelPrefab(BasePanel prefab)
        {
            if (prefab != null) _prefabCache[prefab.GetType()] = prefab;
        }
    }
}
