using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Army;

namespace SmallTroopsBigBattles.UI.Panels
{
    /// <summary>
    /// 軍隊面板
    /// </summary>
    public class ArmyPanel : BasePanel
    {
        [Header("士兵數量顯示")]
        [SerializeField] private TextMeshProUGUI spearmanCountText;
        [SerializeField] private TextMeshProUGUI shieldmanCountText;
        [SerializeField] private TextMeshProUGUI cavalryCountText;
        [SerializeField] private TextMeshProUGUI archerCountText;
        [SerializeField] private TextMeshProUGUI totalCountText;

        [Header("招募介面")]
        [SerializeField] private TMP_Dropdown soldierTypeDropdown;
        [SerializeField] private Slider countSlider;
        [SerializeField] private TMP_InputField countInput;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button recruitButton;

        [Header("招募數量設定")]
        [SerializeField] private int maxRecruitPerTime = 100;

        private SoldierType _selectedType = SoldierType.Spearman;
        private int _recruitCount = 10;

        protected override void Awake()
        {
            base.Awake();

            // 初始化下拉選單
            if (soldierTypeDropdown != null)
            {
                soldierTypeDropdown.ClearOptions();
                soldierTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "槍兵", "盾兵", "騎兵", "弓兵"
                });
                soldierTypeDropdown.onValueChanged.AddListener(OnSoldierTypeChanged);
            }

            // 初始化滑動條
            if (countSlider != null)
            {
                countSlider.minValue = 1;
                countSlider.maxValue = maxRecruitPerTime;
                countSlider.wholeNumbers = true;
                countSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            // 初始化輸入框
            if (countInput != null)
            {
                countInput.onEndEdit.AddListener(OnInputValueChanged);
            }

            // 綁定招募按鈕
            if (recruitButton != null)
            {
                recruitButton.onClick.AddListener(OnRecruitButtonClick);
            }

            // 訂閱事件
            EventManager.Instance?.Subscribe<SoldiersChangedEvent>(OnSoldiersChanged);
        }

        protected override void OnDestroy()
        {
            EventManager.Instance?.Unsubscribe<SoldiersChangedEvent>(OnSoldiersChanged);
            base.OnDestroy();
        }

        protected override void OnShow()
        {
            RefreshData();
            UpdateRecruitUI();
        }

        public override void RefreshData()
        {
            var army = ArmyManager.Instance?.PlayerArmy;
            if (army == null) return;

            if (spearmanCountText != null)
                spearmanCountText.text = $"槍兵: {army.Spearman}";

            if (shieldmanCountText != null)
                shieldmanCountText.text = $"盾兵: {army.Shieldman}";

            if (cavalryCountText != null)
                cavalryCountText.text = $"騎兵: {army.Cavalry}";

            if (archerCountText != null)
                archerCountText.text = $"弓兵: {army.Archer}";

            if (totalCountText != null)
                totalCountText.text = $"總計: {army.TotalSoldiers} / {Game.Data.PlayerArmy.MaxSoldiers}";
        }

        /// <summary>
        /// 更新招募介面
        /// </summary>
        private void UpdateRecruitUI()
        {
            var cost = ArmyManager.Instance?.GetRecruitCost(_selectedType, _recruitCount) ?? (0, 0);

            if (costText != null)
                costText.text = $"消耗: 銅錢 {cost.copper}, 糧草 {cost.food}";

            if (countInput != null)
                countInput.text = _recruitCount.ToString();

            if (countSlider != null)
                countSlider.value = _recruitCount;
        }

        /// <summary>
        /// 兵種選擇變更
        /// </summary>
        private void OnSoldierTypeChanged(int index)
        {
            _selectedType = (SoldierType)index;
            UpdateRecruitUI();
        }

        /// <summary>
        /// 滑動條值變更
        /// </summary>
        private void OnSliderValueChanged(float value)
        {
            _recruitCount = Mathf.RoundToInt(value);
            UpdateRecruitUI();
        }

        /// <summary>
        /// 輸入框值變更
        /// </summary>
        private void OnInputValueChanged(string value)
        {
            if (int.TryParse(value, out int count))
            {
                _recruitCount = Mathf.Clamp(count, 1, maxRecruitPerTime);
                UpdateRecruitUI();
            }
        }

        /// <summary>
        /// 招募按鈕點擊
        /// </summary>
        private void OnRecruitButtonClick()
        {
            int recruited = ArmyManager.Instance?.RecruitSoldiers(_selectedType, _recruitCount) ?? 0;

            if (recruited > 0)
            {
                Debug.Log($"[ArmyPanel] 成功招募 {recruited} 名 {ArmyManager.GetSoldierDisplayName(_selectedType)}");
            }
            else
            {
                Debug.LogWarning("[ArmyPanel] 招募失敗，資源不足或已達上限");
            }
        }

        /// <summary>
        /// 士兵數量變更事件
        /// </summary>
        private void OnSoldiersChanged(SoldiersChangedEvent evt)
        {
            RefreshData();
        }
    }
}

