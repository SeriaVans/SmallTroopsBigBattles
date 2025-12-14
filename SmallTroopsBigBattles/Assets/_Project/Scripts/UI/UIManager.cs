using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// UI 管理器 - 負責面板的創建、顯示和管理
    /// </summary>
    public class UIManager : SingletonBase<UIManager>
    {
        [Header("UI 層級容器")]
        [SerializeField] private Transform normalLayer;
        [SerializeField] private Transform popupLayer;
        [SerializeField] private Transform topLayer;

        [Header("面板預製體")]
        [SerializeField] private List<BasePanel> panelPrefabs = new();

        // 已創建的面板實例
        private readonly Dictionary<string, BasePanel> _panelInstances = new();

        // 面板顯示棧（用於返回功能）
        private readonly Stack<string> _panelStack = new();

        protected override void OnSingletonAwake()
        {
            Debug.Log("[UIManager] UI 管理器初始化完成");
        }

        /// <summary>
        /// 設置 UI 層級容器
        /// </summary>
        public void SetupLayers(Transform normal, Transform popup, Transform top)
        {
            normalLayer = normal;
            popupLayer = popup;
            topLayer = top;
        }

        /// <summary>
        /// 註冊面板預製體
        /// </summary>
        public void RegisterPanelPrefab(BasePanel prefab)
        {
            if (prefab != null && !panelPrefabs.Contains(prefab))
            {
                panelPrefabs.Add(prefab);
            }
        }

        /// <summary>
        /// 獲取層級容器
        /// </summary>
        private Transform GetLayerParent(PanelLayer layer)
        {
            return layer switch
            {
                PanelLayer.Normal => normalLayer,
                PanelLayer.Popup => popupLayer,
                PanelLayer.Top => topLayer,
                _ => normalLayer
            };
        }

        /// <summary>
        /// 打開面板
        /// </summary>
        public T OpenPanel<T>() where T : BasePanel
        {
            string panelName = typeof(T).Name;
            return OpenPanel(panelName) as T;
        }

        /// <summary>
        /// 打開面板（依名稱）
        /// </summary>
        public BasePanel OpenPanel(string panelName)
        {
            // 檢查是否已有實例
            if (_panelInstances.TryGetValue(panelName, out var existingPanel))
            {
                existingPanel.Show();
                PushToStack(panelName);
                return existingPanel;
            }

            // 尋找對應的預製體
            var prefab = panelPrefabs.Find(p => p.PanelName == panelName);
            if (prefab == null)
            {
                Debug.LogWarning($"[UIManager] 找不到面板預製體: {panelName}");
                return null;
            }

            // 創建實例
            var parent = GetLayerParent(prefab.Layer);
            var panel = Instantiate(prefab, parent);
            panel.name = panelName;

            _panelInstances[panelName] = panel;
            panel.Show();
            PushToStack(panelName);

            Debug.Log($"[UIManager] 打開面板: {panelName}");
            return panel;
        }

        /// <summary>
        /// 關閉面板
        /// </summary>
        public void ClosePanel<T>() where T : BasePanel
        {
            ClosePanel(typeof(T).Name);
        }

        /// <summary>
        /// 關閉面板（依名稱）
        /// </summary>
        public void ClosePanel(string panelName)
        {
            if (_panelInstances.TryGetValue(panelName, out var panel))
            {
                panel.Hide();
                PopFromStack(panelName);
                Debug.Log($"[UIManager] 關閉面板: {panelName}");
            }
        }

        /// <summary>
        /// 關閉所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            foreach (var panel in _panelInstances.Values)
            {
                if (panel.IsShowing)
                {
                    panel.Hide();
                }
            }
            _panelStack.Clear();
            Debug.Log("[UIManager] 關閉所有面板");
        }

        /// <summary>
        /// 獲取面板實例
        /// </summary>
        public T GetPanel<T>() where T : BasePanel
        {
            string panelName = typeof(T).Name;
            if (_panelInstances.TryGetValue(panelName, out var panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 檢查面板是否顯示中
        /// </summary>
        public bool IsPanelShowing<T>() where T : BasePanel
        {
            var panel = GetPanel<T>();
            return panel != null && panel.IsShowing;
        }

        /// <summary>
        /// 檢查面板是否顯示中（依名稱）
        /// </summary>
        public bool IsPanelShowing(string panelName)
        {
            if (_panelInstances.TryGetValue(panelName, out var panel))
            {
                return panel.IsShowing;
            }
            return false;
        }

        /// <summary>
        /// 返回上一個面板
        /// </summary>
        public void GoBack()
        {
            if (_panelStack.Count > 0)
            {
                string currentPanel = _panelStack.Pop();
                ClosePanel(currentPanel);

                if (_panelStack.Count > 0)
                {
                    string previousPanel = _panelStack.Peek();
                    if (_panelInstances.TryGetValue(previousPanel, out var panel))
                    {
                        panel.Show();
                    }
                }
            }
        }

        /// <summary>
        /// 將面板加入棧
        /// </summary>
        private void PushToStack(string panelName)
        {
            if (_panelStack.Count == 0 || _panelStack.Peek() != panelName)
            {
                _panelStack.Push(panelName);
            }
        }

        /// <summary>
        /// 將面板從棧移除
        /// </summary>
        private void PopFromStack(string panelName)
        {
            if (_panelStack.Count > 0 && _panelStack.Peek() == panelName)
            {
                _panelStack.Pop();
            }
        }

        /// <summary>
        /// 銷毀面板實例
        /// </summary>
        public void DestroyPanel<T>() where T : BasePanel
        {
            DestroyPanel(typeof(T).Name);
        }

        /// <summary>
        /// 銷毀面板實例（依名稱）
        /// </summary>
        public void DestroyPanel(string panelName)
        {
            if (_panelInstances.TryGetValue(panelName, out var panel))
            {
                Destroy(panel.gameObject);
                _panelInstances.Remove(panelName);
                PopFromStack(panelName);
                Debug.Log($"[UIManager] 銷毀面板: {panelName}");
            }
        }

        /// <summary>
        /// 清理所有面板
        /// </summary>
        public void ClearAllPanels()
        {
            foreach (var panel in _panelInstances.Values)
            {
                if (panel != null)
                {
                    Destroy(panel.gameObject);
                }
            }
            _panelInstances.Clear();
            _panelStack.Clear();
        }

        protected override void OnDestroy()
        {
            ClearAllPanels();
            base.OnDestroy();
        }
    }
}

