using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SmallTroopsBigBattles.Game.Battle;

namespace SmallTroopsBigBattles.UI.Battle
{
    /// <summary>
    /// 戰場視圖 - 顯示戰場上的士兵和戰鬥
    /// </summary>
    public class BattlefieldView : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [Header("設定")]
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject soldierUIPrefab;
        [SerializeField] private Transform soldiersContainer;

        [Header("縮放設定")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float dragSpeed = 1f;

        [Header("顯示設定")]
        [SerializeField] private int maxVisibleSoldiers = 200;
        [SerializeField] private float updateInterval = 0.1f;

        /// <summary>當前戰場</summary>
        private Battlefield _battlefield;

        /// <summary>士兵 UI 對照表</summary>
        private Dictionary<long, SoldierUI> _soldierUIMap = new Dictionary<long, SoldierUI>();

        /// <summary>士兵 UI 對象池</summary>
        private Queue<SoldierUI> _soldierUIPool = new Queue<SoldierUI>();

        /// <summary>當前縮放</summary>
        private float _currentZoom = 1f;

        /// <summary>更新計時器</summary>
        private float _updateTimer;

        /// <summary>視口範圍（用於剔除）</summary>
        private Rect _viewportRect;

        private void Awake()
        {
            // 預先創建對象池
            for (int i = 0; i < maxVisibleSoldiers; i++)
            {
                CreatePooledSoldierUI();
            }
        }

        private void Update()
        {
            if (_battlefield == null) return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= updateInterval)
            {
                UpdateSoldierPositions();
                _updateTimer = 0;
            }
        }

        /// <summary>
        /// 初始化戰場視圖
        /// </summary>
        public void Initialize(Battlefield battlefield)
        {
            _battlefield = battlefield;

            // 設置內容尺寸
            if (content != null)
            {
                content.sizeDelta = battlefield.BattlefieldSize;
            }

            // 居中顯示
            CenterView();

            // 初始顯示
            if (battlefield != null && battlefield.Soldiers != null)
            {
                UpdateSoldierPositions();
            }
        }

        /// <summary>
        /// 居中視圖
        /// </summary>
        public void CenterView()
        {
            if (content == null || _battlefield == null) return;

            content.anchoredPosition = -_battlefield.Center;
        }

        /// <summary>
        /// 更新士兵位置
        /// </summary>
        private void UpdateSoldierPositions()
        {
            if (_battlefield == null || _battlefield.Soldiers == null) return;

            // 計算視口範圍
            UpdateViewportRect();

            // 標記所有現有 UI 為未使用
            var usedIds = new HashSet<long>();

            // 獲取存活的士兵（按距離視口中心排序，限制數量）
            var visibleSoldiers = GetVisibleSoldiers();

            foreach (var soldier in visibleSoldiers)
            {
                if (soldier == null) continue;

                usedIds.Add(soldier.SoldierId);

                if (_soldierUIMap.TryGetValue(soldier.SoldierId, out var soldierUI))
                {
                    // 更新現有 UI
                    if (soldierUI != null)
                    {
                        soldierUI.UpdateFromSoldier(soldier);
                    }
                }
                else
                {
                    // 創建新 UI
                    var newUI = GetSoldierUIFromPool();
                    if (newUI != null)
                    {
                        newUI.Initialize(soldier);
                        _soldierUIMap[soldier.SoldierId] = newUI;
                    }
                }
            }

            // 回收未使用的 UI
            var toRemove = new List<long>();
            foreach (var kvp in _soldierUIMap)
            {
                if (!usedIds.Contains(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        ReturnSoldierUIToPool(kvp.Value);
                    }
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
            {
                _soldierUIMap.Remove(id);
            }
        }

        /// <summary>
        /// 獲取可見的士兵列表
        /// </summary>
        private List<BattleSoldier> GetVisibleSoldiers()
        {
            var result = new List<BattleSoldier>();

            foreach (var soldier in _battlefield.Soldiers)
            {
                if (!soldier.IsAlive) continue;

                // 檢查是否在視口內（加上邊距）
                var expandedRect = new Rect(
                    _viewportRect.x - 100,
                    _viewportRect.y - 100,
                    _viewportRect.width + 200,
                    _viewportRect.height + 200
                );

                if (expandedRect.Contains(soldier.Position))
                {
                    result.Add(soldier);
                }
            }

            // 限制數量
            if (result.Count > maxVisibleSoldiers)
            {
                // 按距離視口中心排序
                var viewCenter = _viewportRect.center;
                result.Sort((a, b) =>
                {
                    float distA = Vector2.Distance(a.Position, viewCenter);
                    float distB = Vector2.Distance(b.Position, viewCenter);
                    return distA.CompareTo(distB);
                });

                result = result.GetRange(0, maxVisibleSoldiers);
            }

            return result;
        }

        /// <summary>
        /// 更新視口範圍
        /// </summary>
        private void UpdateViewportRect()
        {
            if (content == null) return;

            var viewportSize = ((RectTransform)transform).rect.size / _currentZoom;
            var viewportCenter = -content.anchoredPosition;

            _viewportRect = new Rect(
                viewportCenter.x - viewportSize.x / 2,
                viewportCenter.y - viewportSize.y / 2,
                viewportSize.x,
                viewportSize.y
            );
        }

        #region 對象池

        /// <summary>
        /// 創建池化的士兵 UI
        /// </summary>
        private void CreatePooledSoldierUI()
        {
            if (soldierUIPrefab == null || soldiersContainer == null) return;

            var obj = Instantiate(soldierUIPrefab, soldiersContainer);
            var soldierUI = obj.GetComponent<SoldierUI>();

            if (soldierUI == null)
            {
                soldierUI = obj.AddComponent<SoldierUI>();
            }

            obj.SetActive(false);
            _soldierUIPool.Enqueue(soldierUI);
        }

        /// <summary>
        /// 從對象池獲取士兵 UI
        /// </summary>
        private SoldierUI GetSoldierUIFromPool()
        {
            if (_soldierUIPool.Count == 0)
            {
                CreatePooledSoldierUI();
            }

            if (_soldierUIPool.Count > 0)
            {
                var soldierUI = _soldierUIPool.Dequeue();
                soldierUI.gameObject.SetActive(true);
                return soldierUI;
            }

            return null;
        }

        /// <summary>
        /// 歸還士兵 UI 到對象池
        /// </summary>
        private void ReturnSoldierUIToPool(SoldierUI soldierUI)
        {
            if (soldierUI == null) return;

            soldierUI.gameObject.SetActive(false);
            _soldierUIPool.Enqueue(soldierUI);
        }

        #endregion

        #region 拖動和縮放

        public void OnDrag(PointerEventData eventData)
        {
            if (content == null) return;

            Vector2 delta = eventData.delta * dragSpeed / _currentZoom;
            content.anchoredPosition += delta;

            // 限制範圍
            ClampContentPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            float scrollDelta = eventData.scrollDelta.y * zoomSpeed;
            float newZoom = Mathf.Clamp(_currentZoom + scrollDelta, minZoom, maxZoom);

            if (Mathf.Approximately(newZoom, _currentZoom)) return;

            _currentZoom = newZoom;
            content.localScale = Vector3.one * _currentZoom;

            ClampContentPosition();
        }

        /// <summary>
        /// 限制內容位置
        /// </summary>
        private void ClampContentPosition()
        {
            if (content == null || _battlefield == null) return;

            var viewportSize = ((RectTransform)transform).rect.size;
            var contentSize = _battlefield.BattlefieldSize * _currentZoom;

            float minX = -(contentSize.x - viewportSize.x / 2);
            float maxX = viewportSize.x / 2;
            float minY = -(contentSize.y - viewportSize.y / 2);
            float maxY = viewportSize.y / 2;

            var pos = content.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            content.anchoredPosition = pos;
        }

        #endregion

        /// <summary>
        /// 聚焦到指定位置
        /// </summary>
        public void FocusPosition(Vector2 position)
        {
            if (content == null) return;

            content.anchoredPosition = -position;
            ClampContentPosition();
        }

        /// <summary>
        /// 清除所有士兵 UI
        /// </summary>
        public void ClearAllSoldiers()
        {
            foreach (var kvp in _soldierUIMap)
            {
                ReturnSoldierUIToPool(kvp.Value);
            }
            _soldierUIMap.Clear();
        }
    }
}

