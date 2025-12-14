using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Map
{
    /// <summary>
    /// 地圖管理器 - 負責管理所有地圖節點和路徑
    /// </summary>
    public class MapManager : SingletonBase<MapManager>
    {
        /// <summary>所有地圖節點</summary>
        private Dictionary<string, MapNodeData> _nodes = new Dictionary<string, MapNodeData>();

        /// <summary>所有路徑</summary>
        private Dictionary<string, MapRouteData> _routes = new Dictionary<string, MapRouteData>();

        /// <summary>當前選中的節點</summary>
        private MapNodeData _selectedNode;

        /// <summary>節點列表（唯讀）</summary>
        public IReadOnlyDictionary<string, MapNodeData> Nodes => _nodes;

        /// <summary>路徑列表（唯讀）</summary>
        public IReadOnlyDictionary<string, MapRouteData> Routes => _routes;

        /// <summary>當前選中的節點</summary>
        public MapNodeData SelectedNode => _selectedNode;

        /// <summary>
        /// 初始化地圖管理器
        /// </summary>
        public void Initialize()
        {
            _nodes.Clear();
            _routes.Clear();

            // 生成測試地圖
            GenerateTestMap();

            Debug.Log($"[MapManager] 初始化完成，節點數：{_nodes.Count}，路徑數：{_routes.Count}");
        }

        /// <summary>
        /// 生成測試用地圖
        /// </summary>
        private void GenerateTestMap()
        {
            // 創建一個簡單的測試地圖（7x5 網格）
            int width = 7;
            int height = 5;

            // 預定義一些特殊節點位置
            var cityPositions = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 2),   // 左側城池
                new Vector2Int(3, 2),   // 中央城池
                new Vector2Int(6, 2)    // 右側城池
            };

            var passPositions = new HashSet<Vector2Int>
            {
                new Vector2Int(2, 2),   // 左關隘
                new Vector2Int(4, 2)    // 右關隘
            };

            var mountainPositions = new HashSet<Vector2Int>
            {
                new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(4, 0), new Vector2Int(5, 0),
                new Vector2Int(1, 4), new Vector2Int(2, 4),
                new Vector2Int(4, 4), new Vector2Int(5, 4)
            };

            var forestPositions = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(6, 0),
                new Vector2Int(0, 4), new Vector2Int(6, 4),
                new Vector2Int(1, 1), new Vector2Int(5, 1),
                new Vector2Int(1, 3), new Vector2Int(5, 3)
            };

            // 創建節點
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    NodeType nodeType;
                    string nodeName;

                    if (cityPositions.Contains(pos))
                    {
                        nodeType = NodeType.City;
                        nodeName = GetCityName(pos);
                    }
                    else if (passPositions.Contains(pos))
                    {
                        nodeType = NodeType.Pass;
                        nodeName = GetPassName(pos);
                    }
                    else if (mountainPositions.Contains(pos))
                    {
                        nodeType = NodeType.Mountain;
                        nodeName = $"山地 {x}-{y}";
                    }
                    else if (forestPositions.Contains(pos))
                    {
                        nodeType = NodeType.Forest;
                        nodeName = $"森林 {x}-{y}";
                    }
                    else
                    {
                        nodeType = NodeType.Plain;
                        nodeName = $"平原 {x}-{y}";
                    }

                    var node = new MapNodeData(nodeName, nodeType, pos);
                    node.WorldPosition = new Vector3(x * 100f, 0, y * 100f);
                    _nodes[node.NodeId] = node;
                }
            }

            // 創建連接（相鄰節點之間）
            var nodeList = _nodes.Values.ToList();
            foreach (var node in nodeList)
            {
                var pos = node.GridPosition;

                // 檢查四個方向的相鄰節點
                var directions = new[]
                {
                    new Vector2Int(1, 0),   // 右
                    new Vector2Int(-1, 0),  // 左
                    new Vector2Int(0, 1),   // 上
                    new Vector2Int(0, -1)   // 下
                };

                foreach (var dir in directions)
                {
                    var neighborPos = pos + dir;
                    var neighbor = GetNodeByPosition(neighborPos);

                    if (neighbor != null && node.IsPassable && neighbor.IsPassable)
                    {
                        node.AddConnection(neighbor.NodeId);

                        // 創建路徑（只創建一次，避免重複）
                        if (!HasRoute(node.NodeId, neighbor.NodeId))
                        {
                            var distance = CalculateRouteDistance(node, neighbor);
                            var route = new MapRouteData(node.NodeId, neighbor.NodeId, distance);
                            _routes[route.RouteId] = route;
                        }
                    }
                }
            }

            // 設置初始國家控制
            SetInitialNationControl();
        }

        /// <summary>
        /// 獲取城市名稱
        /// </summary>
        private string GetCityName(Vector2Int pos)
        {
            if (pos.x == 0) return "蜀城";
            if (pos.x == 3) return "中都";
            if (pos.x == 6) return "魏城";
            return $"城池 {pos.x}-{pos.y}";
        }

        /// <summary>
        /// 獲取關隘名稱
        /// </summary>
        private string GetPassName(Vector2Int pos)
        {
            if (pos.x == 2) return "劍閣";
            if (pos.x == 4) return "虎牢關";
            return $"關隘 {pos.x}-{pos.y}";
        }

        /// <summary>
        /// 設置初始國家控制
        /// </summary>
        private void SetInitialNationControl()
        {
            foreach (var node in _nodes.Values)
            {
                // 左側區域歸蜀國
                if (node.GridPosition.x < 2)
                {
                    node.SetControllingNation("nation_shu");
                }
                // 右側區域歸魏國
                else if (node.GridPosition.x > 4)
                {
                    node.SetControllingNation("nation_wei");
                }
                // 中間區域歸吳國
                else if (node.GridPosition.x == 3)
                {
                    node.SetControllingNation("nation_wu");
                }
                // 關隘中立
            }
        }

        /// <summary>
        /// 根據網格座標獲取節點
        /// </summary>
        public MapNodeData GetNodeByPosition(Vector2Int position)
        {
            return _nodes.Values.FirstOrDefault(n => n.GridPosition == position);
        }

        /// <summary>
        /// 根據 ID 獲取節點
        /// </summary>
        public MapNodeData GetNode(string nodeId)
        {
            return _nodes.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// 檢查兩個節點之間是否有路徑
        /// </summary>
        public bool HasRoute(string fromNodeId, string toNodeId)
        {
            return _routes.Values.Any(r =>
                (r.FromNodeId == fromNodeId && r.ToNodeId == toNodeId) ||
                (r.IsBidirectional && r.FromNodeId == toNodeId && r.ToNodeId == fromNodeId));
        }

        /// <summary>
        /// 獲取兩個節點之間的路徑
        /// </summary>
        public MapRouteData GetRoute(string fromNodeId, string toNodeId)
        {
            return _routes.Values.FirstOrDefault(r =>
                (r.FromNodeId == fromNodeId && r.ToNodeId == toNodeId) ||
                (r.IsBidirectional && r.FromNodeId == toNodeId && r.ToNodeId == fromNodeId));
        }

        /// <summary>
        /// 計算路徑距離
        /// </summary>
        private float CalculateRouteDistance(MapNodeData from, MapNodeData to)
        {
            // 基礎距離 + 地形移動消耗
            float baseDist = 1.0f;
            float terrainCost = (from.MovementCost + to.MovementCost) / 2f;
            return baseDist * terrainCost;
        }

        /// <summary>
        /// 選擇節點
        /// </summary>
        public void SelectNode(string nodeId)
        {
            _selectedNode = GetNode(nodeId);

            if (_selectedNode != null)
            {
                EventManager.Instance?.Publish(new NodeSelectedEvent
                {
                    Node = _selectedNode
                });
            }
        }

        /// <summary>
        /// 取消選擇
        /// </summary>
        public void DeselectNode()
        {
            _selectedNode = null;
            EventManager.Instance?.Publish(new NodeDeselectedEvent());
        }

        /// <summary>
        /// 獲取指定國家控制的所有節點
        /// </summary>
        public List<MapNodeData> GetNodesByNation(string nationId)
        {
            return _nodes.Values
                .Where(n => n.ControllingNationId == nationId)
                .ToList();
        }

        /// <summary>
        /// 獲取指定類型的所有節點
        /// </summary>
        public List<MapNodeData> GetNodesByType(NodeType type)
        {
            return _nodes.Values
                .Where(n => n.Type == type)
                .ToList();
        }

        /// <summary>
        /// 獲取所有城池節點
        /// </summary>
        public List<MapNodeData> GetAllCities()
        {
            return GetNodesByType(NodeType.City);
        }

        /// <summary>
        /// 獲取所有戰略要地
        /// </summary>
        public List<MapNodeData> GetStrategicPoints()
        {
            return _nodes.Values
                .Where(n => n.IsStrategicPoint)
                .ToList();
        }

        /// <summary>
        /// 計算從起點到終點的路徑（簡單版 A* 算法）
        /// </summary>
        public List<MapNodeData> FindPath(string startNodeId, string endNodeId)
        {
            var startNode = GetNode(startNodeId);
            var endNode = GetNode(endNodeId);

            if (startNode == null || endNode == null)
                return null;

            var openSet = new List<PathNode>();
            var closedSet = new HashSet<string>();
            var cameFrom = new Dictionary<string, string>();

            var startPathNode = new PathNode
            {
                NodeId = startNodeId,
                GCost = 0,
                HCost = GetHeuristic(startNode, endNode)
            };

            openSet.Add(startPathNode);

            while (openSet.Count > 0)
            {
                // 找到 F 值最小的節點
                openSet.Sort((a, b) => a.FCost.CompareTo(b.FCost));
                var current = openSet[0];

                if (current.NodeId == endNodeId)
                {
                    // 重建路徑
                    return ReconstructPath(cameFrom, current.NodeId);
                }

                openSet.RemoveAt(0);
                closedSet.Add(current.NodeId);

                var currentNode = GetNode(current.NodeId);
                foreach (var neighborId in currentNode.ConnectedNodeIds)
                {
                    if (closedSet.Contains(neighborId))
                        continue;

                    var neighborNode = GetNode(neighborId);
                    if (neighborNode == null || !neighborNode.IsPassable)
                        continue;

                    var route = GetRoute(current.NodeId, neighborId);
                    var tentativeG = current.GCost + (route?.Distance ?? 1f);

                    var existingNeighbor = openSet.FirstOrDefault(n => n.NodeId == neighborId);
                    if (existingNeighbor == null)
                    {
                        var newPathNode = new PathNode
                        {
                            NodeId = neighborId,
                            GCost = tentativeG,
                            HCost = GetHeuristic(neighborNode, endNode)
                        };
                        openSet.Add(newPathNode);
                        cameFrom[neighborId] = current.NodeId;
                    }
                    else if (tentativeG < existingNeighbor.GCost)
                    {
                        existingNeighbor.GCost = tentativeG;
                        cameFrom[neighborId] = current.NodeId;
                    }
                }
            }

            return null; // 無法找到路徑
        }

        /// <summary>
        /// 計算啟發式估值（曼哈頓距離）
        /// </summary>
        private float GetHeuristic(MapNodeData from, MapNodeData to)
        {
            return Mathf.Abs(from.GridPosition.x - to.GridPosition.x) +
                   Mathf.Abs(from.GridPosition.y - to.GridPosition.y);
        }

        /// <summary>
        /// 重建路徑
        /// </summary>
        private List<MapNodeData> ReconstructPath(Dictionary<string, string> cameFrom, string currentId)
        {
            var path = new List<MapNodeData>();
            var current = currentId;

            while (cameFrom.ContainsKey(current))
            {
                path.Add(GetNode(current));
                current = cameFrom[current];
            }
            path.Add(GetNode(current));
            path.Reverse();

            return path;
        }

        /// <summary>
        /// 路徑尋找輔助類
        /// </summary>
        private class PathNode
        {
            public string NodeId;
            public float GCost; // 從起點到該節點的成本
            public float HCost; // 從該節點到終點的估計成本
            public float FCost => GCost + HCost;
        }
    }

    #region 地圖相關事件

    /// <summary>
    /// 節點被選中事件
    /// </summary>
    public class NodeSelectedEvent : Core.Events.GameEvent
    {
        public MapNodeData Node;
    }

    /// <summary>
    /// 節點取消選中事件
    /// </summary>
    public class NodeDeselectedEvent : Core.Events.GameEvent { }

    /// <summary>
    /// 節點控制權變更事件
    /// </summary>
    public class NodeControlChangedEvent : Core.Events.GameEvent
    {
        public MapNodeData Node;
        public string PreviousNationId;
        public string NewNationId;
    }

    #endregion
}

