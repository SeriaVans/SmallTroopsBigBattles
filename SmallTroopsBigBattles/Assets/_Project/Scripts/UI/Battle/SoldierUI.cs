using UnityEngine;
using UnityEngine.UI;
using SmallTroopsBigBattles.Game.Battle;

namespace SmallTroopsBigBattles.UI.Battle
{
    /// <summary>
    /// 士兵 UI - 單個士兵的視覺表現
    /// </summary>
    public class SoldierUI : MonoBehaviour
    {
        [Header("UI 元件")]
        [SerializeField] private Image soldierImage;
        [SerializeField] private Image healthBar;
        [SerializeField] private Image healthBarBg;
        [SerializeField] private RectTransform rectTransform;

        [Header("兵種顏色")]
        [SerializeField] private Color spearmanColor = new Color(0.8f, 0.6f, 0.2f);
        [SerializeField] private Color shieldmanColor = new Color(0.5f, 0.5f, 0.7f);
        [SerializeField] private Color cavalryColor = new Color(0.6f, 0.3f, 0.2f);
        [SerializeField] private Color archerColor = new Color(0.2f, 0.6f, 0.2f);

        [Header("國家顏色")]
        [SerializeField] private Color shuColor = new Color(0.2f, 0.7f, 0.3f);
        [SerializeField] private Color weiColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color wuColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("狀態顏色")]
        [SerializeField] private Color retreatingColor = new Color(0.8f, 0.8f, 0.3f);

        /// <summary>關聯的士兵數據</summary>
        private BattleSoldier _soldier;

        /// <summary>目標位置（用於平滑移動）</summary>
        private Vector2 _targetPosition;

        /// <summary>移動平滑速度</summary>
        private const float SmoothSpeed = 10f;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (soldierImage == null)
                soldierImage = GetComponent<Image>();
        }

        private void Update()
        {
            // 平滑移動到目標位置
            if (_soldier != null && rectTransform != null)
            {
                Vector2 currentPos = rectTransform.anchoredPosition;
                Vector2 newPos = Vector2.Lerp(currentPos, _targetPosition, Time.deltaTime * SmoothSpeed);
                rectTransform.anchoredPosition = newPos;
            }
        }

        /// <summary>
        /// 初始化士兵 UI
        /// </summary>
        public void Initialize(BattleSoldier soldier)
        {
            _soldier = soldier;
            _targetPosition = soldier.Position;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = soldier.Position;
            }

            UpdateVisual();
        }

        /// <summary>
        /// 從士兵數據更新
        /// </summary>
        public void UpdateFromSoldier(BattleSoldier soldier)
        {
            _soldier = soldier;
            _targetPosition = soldier.Position;

            UpdateVisual();
        }

        /// <summary>
        /// 更新視覺效果
        /// </summary>
        private void UpdateVisual()
        {
            if (_soldier == null) return;

            UpdateColor();
            UpdateHealthBar();
            UpdateSize();
            UpdateRotation();
        }

        /// <summary>
        /// 更新顏色
        /// </summary>
        private void UpdateColor()
        {
            if (soldierImage == null) return;

            // 撤退中使用特殊顏色
            if (_soldier.State == SoldierState.Retreating)
            {
                soldierImage.color = retreatingColor;
                return;
            }

            // 根據國家設置邊框顏色（通過透明度調整）
            Color baseColor = GetSoldierTypeColor(_soldier.Type);
            Color nationColor = GetNationColor(_soldier.NationId);

            // 混合顏色
            soldierImage.color = Color.Lerp(baseColor, nationColor, 0.5f);
        }

        /// <summary>
        /// 獲取兵種顏色
        /// </summary>
        private Color GetSoldierTypeColor(SoldierType type)
        {
            return type switch
            {
                SoldierType.Spearman => spearmanColor,
                SoldierType.Shieldman => shieldmanColor,
                SoldierType.Cavalry => cavalryColor,
                SoldierType.Archer => archerColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// 獲取國家顏色
        /// </summary>
        private Color GetNationColor(string nationId)
        {
            return nationId switch
            {
                "nation_shu" => shuColor,
                "nation_wei" => weiColor,
                "nation_wu" => wuColor,
                _ => Color.gray
            };
        }

        /// <summary>
        /// 更新血條
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBar == null) return;

            float healthPercent = _soldier.CurrentHp / _soldier.MaxHp;
            healthBar.fillAmount = healthPercent;

            // 根據血量改變顏色
            if (healthPercent > 0.6f)
            {
                healthBar.color = Color.green;
            }
            else if (healthPercent > 0.3f)
            {
                healthBar.color = Color.yellow;
            }
            else
            {
                healthBar.color = Color.red;
            }

            // 低血量時顯示血條，否則隱藏
            bool showHealthBar = healthPercent < 1f;
            if (healthBarBg != null)
                healthBarBg.gameObject.SetActive(showHealthBar);
        }

        /// <summary>
        /// 更新大小（根據兵種）
        /// </summary>
        private void UpdateSize()
        {
            if (rectTransform == null) return;

            float size = _soldier.Type switch
            {
                SoldierType.Cavalry => 20f,   // 騎兵較大
                SoldierType.Shieldman => 16f, // 盾兵中等
                SoldierType.Spearman => 14f,  // 槍兵中等
                SoldierType.Archer => 12f,    // 弓兵較小
                _ => 14f
            };

            rectTransform.sizeDelta = new Vector2(size, size);
        }

        /// <summary>
        /// 更新旋轉（根據移動方向）
        /// </summary>
        private void UpdateRotation()
        {
            if (rectTransform == null) return;

            // 根據狀態決定朝向
            if (_soldier.State == SoldierState.Retreating)
            {
                // 撤退時朝向後方
                // 這裡簡化處理，實際可以根據目標位置計算
            }

            // 簡單處理：不旋轉
            rectTransform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 播放攻擊動畫（可擴展）
        /// </summary>
        public void PlayAttackAnimation()
        {
            // TODO: 實現攻擊動畫
        }

        /// <summary>
        /// 播放受傷動畫（可擴展）
        /// </summary>
        public void PlayHitAnimation()
        {
            // TODO: 實現受傷動畫（閃爍紅色等）
        }

        /// <summary>
        /// 播放死亡動畫（可擴展）
        /// </summary>
        public void PlayDeathAnimation()
        {
            // TODO: 實現死亡動畫
        }
    }
}

