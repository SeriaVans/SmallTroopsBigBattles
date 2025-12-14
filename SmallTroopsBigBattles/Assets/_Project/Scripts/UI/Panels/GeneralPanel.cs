using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;
using SmallTroopsBigBattles.Game.General;

namespace SmallTroopsBigBattles.UI.Panels
{
    /// <summary>
    /// 將領面板
    /// </summary>
    public class GeneralPanel : BasePanel
    {
        [Header("將領列表")]
        [SerializeField] private Transform generalListContainer;
        [SerializeField] private Button generalListItemPrefab;
        [SerializeField] private TextMeshProUGUI generalCountText;

        [Header("將領詳情")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI generalNameText;
        [SerializeField] private TextMeshProUGUI generalClassText;
        [SerializeField] private TextMeshProUGUI generalLevelText;
        [SerializeField] private TextMeshProUGUI generalExpText;
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI commandText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI maxTroopsText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private Transform starContainer;
        [SerializeField] private Image starPrefab;

        [Header("操作按鈕")]
        [SerializeField] private Button normalRecruitButton;
        [SerializeField] private Button premiumRecruitButton;
        [SerializeField] private Button dismissButton;

        private List<Button> _generalListItems = new();
        private GeneralData _selectedGeneral;

        protected override void Awake()
        {
            base.Awake();

            if (normalRecruitButton != null)
                normalRecruitButton.onClick.AddListener(OnNormalRecruitClick);

            if (premiumRecruitButton != null)
                premiumRecruitButton.onClick.AddListener(OnPremiumRecruitClick);

            if (dismissButton != null)
                dismissButton.onClick.AddListener(OnDismissClick);

            // 訂閱事件
            EventManager.Instance?.Subscribe<GeneralObtainedEvent>(OnGeneralObtained);
            EventManager.Instance?.Subscribe<GeneralLevelUpEvent>(OnGeneralLevelUp);
        }

        protected override void OnDestroy()
        {
            EventManager.Instance?.Unsubscribe<GeneralObtainedEvent>(OnGeneralObtained);
            EventManager.Instance?.Unsubscribe<GeneralLevelUpEvent>(OnGeneralLevelUp);
            base.OnDestroy();
        }

        protected override void OnShow()
        {
            RefreshData();
            HideDetail();
        }

        public override void RefreshData()
        {
            RefreshGeneralList();
            UpdateGeneralCount();
        }

        /// <summary>
        /// 刷新將領列表
        /// </summary>
        private void RefreshGeneralList()
        {
            if (generalListContainer == null || generalListItemPrefab == null) return;

            // 清除現有列表
            foreach (var item in _generalListItems)
            {
                Destroy(item.gameObject);
            }
            _generalListItems.Clear();

            var generals = GeneralManager.Instance?.Generals;
            if (generals == null) return;

            // 創建列表項目
            foreach (var general in generals)
            {
                var item = Instantiate(generalListItemPrefab, generalListContainer);

                var itemText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (itemText != null)
                {
                    string stars = new string('★', general.Rarity);
                    itemText.text = $"{stars} {general.Name}\nLv{general.Level} {GeneralManager.GetClassDisplayName(general.Class)}";
                }

                // 根據稀有度設置顏色
                var colors = item.colors;
                colors.normalColor = GetRarityColor(general.Rarity);
                item.colors = colors;

                // 綁定點擊事件
                var generalRef = general;
                item.onClick.AddListener(() => OnGeneralItemClick(generalRef));

                _generalListItems.Add(item);
            }
        }

        /// <summary>
        /// 更新將領數量
        /// </summary>
        private void UpdateGeneralCount()
        {
            int count = GeneralManager.Instance?.Generals.Count ?? 0;
            if (generalCountText != null)
                generalCountText.text = $"將領: {count}";
        }

        /// <summary>
        /// 獲取稀有度顏色
        /// </summary>
        private Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                1 => new Color(0.6f, 0.6f, 0.6f),   // 灰色
                2 => new Color(0.2f, 0.8f, 0.2f),   // 綠色
                3 => new Color(0.2f, 0.4f, 1f),     // 藍色
                4 => new Color(0.6f, 0.2f, 1f),     // 紫色
                5 => new Color(1f, 0.84f, 0f),      // 金色
                _ => Color.white
            };
        }

        /// <summary>
        /// 將領列表項目點擊
        /// </summary>
        private void OnGeneralItemClick(GeneralData general)
        {
            _selectedGeneral = general;
            ShowDetail(general);
        }

        /// <summary>
        /// 顯示將領詳情
        /// </summary>
        private void ShowDetail(GeneralData general)
        {
            if (detailPanel == null) return;

            if (generalNameText != null)
                generalNameText.text = general.Name;

            if (generalClassText != null)
                generalClassText.text = GeneralManager.GetClassDisplayName(general.Class);

            if (generalLevelText != null)
                generalLevelText.text = $"Lv{general.Level}";

            if (generalExpText != null)
                generalExpText.text = $"經驗: {general.Experience}/{general.GetExpForNextLevel()}";

            if (strengthText != null)
                strengthText.text = $"武力: {general.Strength:F0}";

            if (intelligenceText != null)
                intelligenceText.text = $"智力: {general.Intelligence:F0}";

            if (commandText != null)
                commandText.text = $"統帥: {general.Command:F0}";

            if (speedText != null)
                speedText.text = $"速度: {general.Speed:F0}";

            if (maxTroopsText != null)
                maxTroopsText.text = $"帶兵上限: {general.MaxTroops}";

            if (powerText != null)
                powerText.text = $"戰力: {general.Power}";

            // 顯示星級
            RefreshStars(general.Stars);

            if (dismissButton != null)
                dismissButton.interactable = true;

            detailPanel.SetActive(true);
        }

        /// <summary>
        /// 刷新星級顯示
        /// </summary>
        private void RefreshStars(int stars)
        {
            if (starContainer == null || starPrefab == null) return;

            // 清除現有星星
            foreach (Transform child in starContainer)
            {
                Destroy(child.gameObject);
            }

            // 創建新星星
            for (int i = 0; i < stars; i++)
            {
                var star = Instantiate(starPrefab, starContainer);
                star.color = Color.yellow;
            }
        }

        /// <summary>
        /// 隱藏詳情
        /// </summary>
        private void HideDetail()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);

            _selectedGeneral = null;
        }

        /// <summary>
        /// 普通招募點擊
        /// </summary>
        private void OnNormalRecruitClick()
        {
            var general = GeneralManager.Instance?.NormalRecruit();
            if (general != null)
            {
                Debug.Log($"[GeneralPanel] 招募成功: {general.Name}");
            }
        }

        /// <summary>
        /// 高級招募點擊
        /// </summary>
        private void OnPremiumRecruitClick()
        {
            var general = GeneralManager.Instance?.PremiumRecruit();
            if (general != null)
            {
                Debug.Log($"[GeneralPanel] 高級招募成功: {general.Name}");
            }
        }

        /// <summary>
        /// 遣散點擊
        /// </summary>
        private void OnDismissClick()
        {
            if (_selectedGeneral == null) return;

            GeneralManager.Instance?.DismissGeneral(_selectedGeneral.GeneralId);
            HideDetail();
            RefreshData();
        }

        // 事件處理
        private void OnGeneralObtained(GeneralObtainedEvent evt)
        {
            RefreshData();
        }

        private void OnGeneralLevelUp(GeneralLevelUpEvent evt)
        {
            if (_selectedGeneral != null && _selectedGeneral.GeneralId == evt.GeneralId)
            {
                ShowDetail(_selectedGeneral);
            }
        }
    }
}

