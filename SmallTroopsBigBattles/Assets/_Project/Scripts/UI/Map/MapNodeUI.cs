using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SmallTroopsBigBattles.Game.Map;
using SmallTroopsBigBattles.Game.City;

namespace SmallTroopsBigBattles.UI.Map
{
    /// <summary>
    /// 地圖節點 UI - 單個節點的顯示和互動
    /// </summary>
    public class MapNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI 引用")]
        [SerializeField] private Image nodeImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private Image typeIcon;

        [Header("顏色設定")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f);
        [SerializeField] private Color selectedColor = new Color(0.5f, 1f, 0.5f);

        /// <summary>節點數據</summary>
        private MapNodeData _nodeData;

        /// <summary>是否被選中</summary>
        private bool _isSelected;

        /// <summary>節點數據</summary>
        public MapNodeData NodeData => _nodeData;

        /// <summary>
        /// 設置節點數據
        /// </summary>
        public void SetNodeData(MapNodeData data)
        {
            _nodeData = data;
            UpdateVisual();
        }

        /// <summary>
        /// 更新視覺顯示
        /// </summary>
        public void UpdateVisual()
        {
            if (_nodeData == null) return;

            // 設置節點名稱
            if (nodeNameText != null)
            {
                nodeNameText.text = _nodeData.NodeName;
            }

            // 設置節點顏色（根據控制國家）
            UpdateNationColor();

            // 設置節點外觀（根據類型）
            UpdateNodeTypeVisual();
        }

        /// <summary>
        /// 更新國家顏色
        /// </summary>
        private void UpdateNationColor()
        {
            if (borderImage == null) return;

            if (string.IsNullOrEmpty(_nodeData.ControllingNationId))
            {
                // 中立 - 灰色
                borderImage.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                // 獲取國家顏色
                var nation = NationManager.Instance?.GetNation(_nodeData.ControllingNationId);
                if (nation != null)
                {
                    borderImage.color = nation.NationColor;
                }
            }
        }

        /// <summary>
        /// 更新節點類型視覺
        /// </summary>
        private void UpdateNodeTypeVisual()
        {
            if (nodeImage == null) return;

            Color typeColor = _nodeData.Type switch
            {
                NodeType.Plain => new Color(0.7f, 0.8f, 0.5f),      // 淺綠
                NodeType.Mountain => new Color(0.5f, 0.4f, 0.3f),    // 棕色
                NodeType.Forest => new Color(0.2f, 0.5f, 0.2f),      // 深綠
                NodeType.River => new Color(0.3f, 0.5f, 0.8f),       // 藍色
                NodeType.City => new Color(0.9f, 0.8f, 0.5f),        // 金黃
                NodeType.Pass => new Color(0.6f, 0.3f, 0.3f),        // 暗紅
                NodeType.ResourcePoint => new Color(0.8f, 0.6f, 0.2f), // 橙色
                _ => Color.white
            };

            nodeImage.color = typeColor;

            // 戰略要地加大尺寸
            if (_nodeData.IsStrategicPoint)
            {
                transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 設置選中狀態
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (nodeImage != null)
            {
                // 選中時添加高亮效果
                var currentColor = nodeImage.color;
                if (selected)
                {
                    nodeImage.color = new Color(
                        Mathf.Min(currentColor.r + 0.2f, 1f),
                        Mathf.Min(currentColor.g + 0.2f, 1f),
                        Mathf.Min(currentColor.b + 0.2f, 1f)
                    );
                }
                else
                {
                    UpdateNodeTypeVisual();
                }
            }
        }

        #region UI 事件

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_nodeData == null) return;

            // 選擇該節點
            MapManager.Instance?.SelectNode(_nodeData.NodeId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_nodeData == null || _isSelected) return;

            // 懸停高亮
            if (nodeImage != null)
            {
                var color = nodeImage.color;
                nodeImage.color = new Color(
                    Mathf.Min(color.r + 0.1f, 1f),
                    Mathf.Min(color.g + 0.1f, 1f),
                    Mathf.Min(color.b + 0.1f, 1f)
                );
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_nodeData == null || _isSelected) return;

            // 恢復原色
            UpdateNodeTypeVisual();
        }

        #endregion
    }
}

