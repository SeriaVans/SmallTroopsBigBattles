using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Map;
using SmallTroopsBigBattles.Game.City;

namespace SmallTroopsBigBattles.UI.Map
{
    /// <summary>
    /// 地圖視圖控制器 - 管理整個地圖的 UI 顯示
    /// </summary>
    public class MapViewController : MonoBehaviour
    {
        [Header("UI 引用")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private GameObject nodeUIPrefab;
        [SerializeField] private RectTransform routeContainer;
        [SerializeField] private GameObject routeLinePrefab;

        [Header("設定")]
        [SerializeField] private float nodeSpacing = 100f;
        [SerializeField] private Vector2 mapOffset = new Vector2(50f, 50f);

        /// <summary>節點 UI 對照表</summary>
        private Dictionary<string, MapNodeUI> _nodeUIMap = new Dictionary<string, MapNodeUI>();

        /// <summary>路徑線條對照表</summary>
        private Dictionary<string, GameObject> _routeLines = new Dictionary<string, GameObject>();

        /// <summary>當前選中的節點 UI</summary>
        private MapNodeUI _selectedNodeUI;

        private void Start()
        {
            // 訂閱事件
            EventManager.Instance?.Subscribe<GameInitializedEvent>(OnGameInitialized);
            EventManager.Instance?.Subscribe<NodeSelectedEvent>(OnNodeSelected);
            EventManager.Instance?.Subscribe<NodeDeselectedEvent>(OnNodeDeselected);
            EventManager.Instance?.Subscribe<NodeControlChangedEvent>(OnNodeControlChanged);
        }

        private void OnDestroy()
        {
            EventManager.Instance?.Unsubscribe<GameInitializedEvent>(OnGameInitialized);
            EventManager.Instance?.Unsubscribe<NodeSelectedEvent>(OnNodeSelected);
            EventManager.Instance?.Unsubscribe<NodeDeselectedEvent>(OnNodeDeselected);
            EventManager.Instance?.Unsubscribe<NodeControlChangedEvent>(OnNodeControlChanged);
        }

        /// <summary>
        /// 遊戲初始化後生成地圖
        /// </summary>
        private void OnGameInitialized(GameInitializedEvent evt)
        {
            GenerateMapUI();
        }

        /// <summary>
        /// 生成地圖 UI
        /// </summary>
        public void GenerateMapUI()
        {
            ClearMap();

            var mapManager = MapManager.Instance;
            if (mapManager == null)
            {
                Debug.LogError("[MapViewController] MapManager 未初始化！");
                return;
            }

            // 生成節點 UI
            foreach (var node in mapManager.Nodes.Values)
            {
                CreateNodeUI(node);
            }

            // 生成路徑線條
            foreach (var route in mapManager.Routes.Values)
            {
                CreateRouteLineUI(route);
            }

            Debug.Log($"[MapViewController] 地圖 UI 生成完成，節點數：{_nodeUIMap.Count}");
        }

        /// <summary>
        /// 創建節點 UI
        /// </summary>
        private void CreateNodeUI(MapNodeData nodeData)
        {
            if (nodeUIPrefab == null || mapContainer == null)
            {
                Debug.LogWarning("[MapViewController] nodeUIPrefab 或 mapContainer 未設置！");
                return;
            }

            // 實例化節點 UI
            var nodeObj = Instantiate(nodeUIPrefab, mapContainer);
            var nodeUI = nodeObj.GetComponent<MapNodeUI>();

            if (nodeUI == null)
            {
                nodeUI = nodeObj.AddComponent<MapNodeUI>();
            }

            // 設置位置
            var rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 position = new Vector2(
                    nodeData.GridPosition.x * nodeSpacing + mapOffset.x,
                    nodeData.GridPosition.y * nodeSpacing + mapOffset.y
                );
                rectTransform.anchoredPosition = position;
            }

            // 設置數據
            nodeUI.SetNodeData(nodeData);
            nodeObj.name = $"Node_{nodeData.NodeName}";

            _nodeUIMap[nodeData.NodeId] = nodeUI;
        }

        /// <summary>
        /// 創建路徑線條 UI
        /// </summary>
        private void CreateRouteLineUI(MapRouteData routeData)
        {
            if (routeLinePrefab == null || routeContainer == null) return;

            // 獲取起點和終點的 UI 位置
            if (!_nodeUIMap.TryGetValue(routeData.FromNodeId, out var fromUI) ||
                !_nodeUIMap.TryGetValue(routeData.ToNodeId, out var toUI))
            {
                return;
            }

            var fromRect = fromUI.GetComponent<RectTransform>();
            var toRect = toUI.GetComponent<RectTransform>();

            if (fromRect == null || toRect == null) return;

            // 創建線條
            var lineObj = Instantiate(routeLinePrefab, routeContainer);
            lineObj.name = $"Route_{routeData.RouteId}";

            // 設置線條位置和旋轉
            var lineRect = lineObj.GetComponent<RectTransform>();
            if (lineRect != null)
            {
                Vector2 fromPos = fromRect.anchoredPosition;
                Vector2 toPos = toRect.anchoredPosition;
                Vector2 direction = toPos - fromPos;
                float distance = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // 設置線條中心位置
                lineRect.anchoredPosition = (fromPos + toPos) / 2f;

                // 設置線條長度和旋轉
                lineRect.sizeDelta = new Vector2(distance, 4f);
                lineRect.rotation = Quaternion.Euler(0, 0, angle);
            }

            // 設置線條顏色
            var lineImage = lineObj.GetComponent<Image>();
            if (lineImage != null)
            {
                lineImage.color = routeData.IsPassable
                    ? new Color(0.6f, 0.6f, 0.6f, 0.5f)
                    : new Color(0.8f, 0.2f, 0.2f, 0.5f);
            }

            _routeLines[routeData.RouteId] = lineObj;
        }

        /// <summary>
        /// 清除地圖
        /// </summary>
        public void ClearMap()
        {
            // 清除節點 UI
            foreach (var nodeUI in _nodeUIMap.Values)
            {
                if (nodeUI != null)
                {
                    Destroy(nodeUI.gameObject);
                }
            }
            _nodeUIMap.Clear();

            // 清除路徑線條
            foreach (var line in _routeLines.Values)
            {
                if (line != null)
                {
                    Destroy(line);
                }
            }
            _routeLines.Clear();

            _selectedNodeUI = null;
        }

        /// <summary>
        /// 刷新所有節點顯示
        /// </summary>
        public void RefreshAllNodes()
        {
            foreach (var nodeUI in _nodeUIMap.Values)
            {
                nodeUI?.UpdateVisual();
            }
        }

        /// <summary>
        /// 節點選中事件處理
        /// </summary>
        private void OnNodeSelected(NodeSelectedEvent evt)
        {
            // 取消前一個選中
            if (_selectedNodeUI != null)
            {
                _selectedNodeUI.SetSelected(false);
            }

            // 設置新選中
            if (_nodeUIMap.TryGetValue(evt.Node.NodeId, out var nodeUI))
            {
                _selectedNodeUI = nodeUI;
                _selectedNodeUI.SetSelected(true);
            }
        }

        /// <summary>
        /// 節點取消選中事件處理
        /// </summary>
        private void OnNodeDeselected(NodeDeselectedEvent evt)
        {
            if (_selectedNodeUI != null)
            {
                _selectedNodeUI.SetSelected(false);
                _selectedNodeUI = null;
            }
        }

        /// <summary>
        /// 節點控制權變更事件處理
        /// </summary>
        private void OnNodeControlChanged(NodeControlChangedEvent evt)
        {
            if (_nodeUIMap.TryGetValue(evt.Node.NodeId, out var nodeUI))
            {
                nodeUI.UpdateVisual();
            }
        }

        /// <summary>
        /// 定位到指定節點
        /// </summary>
        public void FocusNode(string nodeId)
        {
            if (!_nodeUIMap.TryGetValue(nodeId, out var nodeUI)) return;

            var nodeRect = nodeUI.GetComponent<RectTransform>();
            if (nodeRect == null || mapContainer == null) return;

            // 計算需要移動的位置（使節點居中）
            var containerParent = mapContainer.parent as RectTransform;
            if (containerParent != null)
            {
                var centerPos = containerParent.rect.center;
                var targetPos = centerPos - nodeRect.anchoredPosition;
                mapContainer.anchoredPosition = targetPos;
            }
        }

        /// <summary>
        /// 獲取節點 UI
        /// </summary>
        public MapNodeUI GetNodeUI(string nodeId)
        {
            return _nodeUIMap.TryGetValue(nodeId, out var nodeUI) ? nodeUI : null;
        }
    }
}

