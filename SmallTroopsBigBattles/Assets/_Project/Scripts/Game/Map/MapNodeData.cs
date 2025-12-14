using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallTroopsBigBattles.Game.Map
{
    /// <summary>
    /// 節點類型
    /// </summary>
    public enum NodeType
    {
        Plain,          // 平原
        Mountain,       // 山地
        Forest,         // 森林
        River,          // 河流/渡口
        City,           // 城池
        Pass,           // 關隘
        ResourcePoint   // 資源點
    }

    /// <summary>
    /// 節點控制狀態
    /// </summary>
    public enum NodeControlState
    {
        Neutral,        // 中立
        Contested,      // 爭奪中
        Controlled      // 被控制
    }

    /// <summary>
    /// 地圖節點數據
    /// </summary>
    [Serializable]
    public class MapNodeData
    {
        /// <summary>節點唯一 ID</summary>
        public string NodeId;

        /// <summary>節點顯示名稱</summary>
        public string NodeName;

        /// <summary>節點類型</summary>
        public NodeType Type;

        /// <summary>地圖座標</summary>
        public Vector2Int GridPosition;

        /// <summary>世界座標</summary>
        public Vector3 WorldPosition;

        /// <summary>控制狀態</summary>
        public NodeControlState ControlState;

        /// <summary>控制該節點的國家 ID（如果有）</summary>
        public string ControllingNationId;

        /// <summary>連接的相鄰節點 ID 列表</summary>
        public List<string> ConnectedNodeIds = new List<string>();

        /// <summary>防禦值（影響佔領難度）</summary>
        public int DefenseValue;

        /// <summary>資源產出（每小時）</summary>
        public ResourceProduction ResourceProduction;

        /// <summary>是否可通行</summary>
        public bool IsPassable = true;

        /// <summary>地形移動消耗係數（1.0 為基準）</summary>
        public float MovementCost = 1.0f;

        /// <summary>是否為戰略要地</summary>
        public bool IsStrategicPoint;

        /// <summary>
        /// 建構函式
        /// </summary>
        public MapNodeData()
        {
            NodeId = Guid.NewGuid().ToString();
            ResourceProduction = new ResourceProduction();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public MapNodeData(string name, NodeType type, Vector2Int gridPos)
        {
            NodeId = Guid.NewGuid().ToString();
            NodeName = name;
            Type = type;
            GridPosition = gridPos;
            ResourceProduction = new ResourceProduction();
            InitializeByType();
        }

        /// <summary>
        /// 根據節點類型初始化屬性
        /// </summary>
        private void InitializeByType()
        {
            switch (Type)
            {
                case NodeType.Plain:
                    MovementCost = 1.0f;
                    DefenseValue = 0;
                    break;

                case NodeType.Mountain:
                    MovementCost = 2.0f;
                    DefenseValue = 30;
                    IsStrategicPoint = true;
                    break;

                case NodeType.Forest:
                    MovementCost = 1.5f;
                    DefenseValue = 15;
                    ResourceProduction.Wood = 50;
                    break;

                case NodeType.River:
                    MovementCost = 1.8f;
                    DefenseValue = 10;
                    break;

                case NodeType.City:
                    MovementCost = 1.0f;
                    DefenseValue = 50;
                    IsStrategicPoint = true;
                    ResourceProduction.Copper = 100;
                    ResourceProduction.Food = 80;
                    break;

                case NodeType.Pass:
                    MovementCost = 1.2f;
                    DefenseValue = 80;
                    IsStrategicPoint = true;
                    break;

                case NodeType.ResourcePoint:
                    MovementCost = 1.0f;
                    DefenseValue = 10;
                    break;
            }
        }

        /// <summary>
        /// 檢查是否與指定節點相鄰
        /// </summary>
        public bool IsAdjacentTo(string otherNodeId)
        {
            return ConnectedNodeIds.Contains(otherNodeId);
        }

        /// <summary>
        /// 添加相鄰節點連接
        /// </summary>
        public void AddConnection(string nodeId)
        {
            if (!ConnectedNodeIds.Contains(nodeId))
            {
                ConnectedNodeIds.Add(nodeId);
            }
        }

        /// <summary>
        /// 移除相鄰節點連接
        /// </summary>
        public void RemoveConnection(string nodeId)
        {
            ConnectedNodeIds.Remove(nodeId);
        }

        /// <summary>
        /// 設置控制國家
        /// </summary>
        public void SetControllingNation(string nationId)
        {
            ControllingNationId = nationId;
            ControlState = string.IsNullOrEmpty(nationId)
                ? NodeControlState.Neutral
                : NodeControlState.Controlled;
        }
    }

    /// <summary>
    /// 資源產出數據
    /// </summary>
    [Serializable]
    public class ResourceProduction
    {
        public int Copper;  // 銅錢產出/小時
        public int Wood;    // 木材產出/小時
        public int Stone;   // 石頭產出/小時
        public int Food;    // 糧草產出/小時

        /// <summary>
        /// 是否有任何資源產出
        /// </summary>
        public bool HasProduction =>
            Copper > 0 || Wood > 0 || Stone > 0 || Food > 0;
    }

    /// <summary>
    /// 節點之間的路徑數據
    /// </summary>
    [Serializable]
    public class MapRouteData
    {
        /// <summary>路徑唯一 ID</summary>
        public string RouteId;

        /// <summary>起始節點 ID</summary>
        public string FromNodeId;

        /// <summary>目標節點 ID</summary>
        public string ToNodeId;

        /// <summary>路徑距離（影響行軍時間）</summary>
        public float Distance;

        /// <summary>是否為雙向路徑</summary>
        public bool IsBidirectional = true;

        /// <summary>路徑是否可通行</summary>
        public bool IsPassable = true;

        /// <summary>
        /// 建構函式
        /// </summary>
        public MapRouteData()
        {
            RouteId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 建構函式（帶參數）
        /// </summary>
        public MapRouteData(string fromNode, string toNode, float distance = 1.0f)
        {
            RouteId = Guid.NewGuid().ToString();
            FromNodeId = fromNode;
            ToNodeId = toNode;
            Distance = distance;
        }
    }
}

