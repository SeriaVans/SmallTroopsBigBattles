using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Managers;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 將領面板 - 將領列表與管理
    /// </summary>
    public class GeneralPanel : BasePanel
    {
        [Header("將領列表")]
        [SerializeField] private Transform _generalListContainer;
        [SerializeField] private GeneralListItemUI _generalItemPrefab;
        [SerializeField] private TextMeshProUGUI _generalCountText;

        [Header("將領詳情")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TextMeshProUGUI _generalNameText;
        [SerializeField] private TextMeshProUGUI _generalClassText;
        [SerializeField] private TextMeshProUGUI _generalLevelText;
        [SerializeField] private TextMeshProUGUI _generalExpText;
        [SerializeField] private Image _expProgressBar;

        [Header("屬性顯示")]
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private TextMeshProUGUI _intelligenceText;
        [SerializeField] private TextMeshProUGUI _commandText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _maxTroopsText;
        [SerializeField] private TextMeshProUGUI _powerText;

        [Header("稀有度星星")]
        [SerializeField] private Transform _starContainer;
        [SerializeField] private Image _starPrefab;

        [Header("操作按鈕")]
        [SerializeField] private Button _recruitButton;
        [SerializeField] private Button _dismissButton;

        private GeneralData _selectedGeneral;
        private List<GeneralListItemUI> _listItems = new List<GeneralListItemUI>();

        protected override void Awake()
        {
            base.Awake();

            if (_recruitButton != null)
                _recruitButton.onClick.AddListener(OnRecruitClicked);

            if (_dismissButton != null)
                _dismissButton.onClick.AddListener(OnDismissClicked);
        }

        protected override void OnPanelOpened()
        {
            RefreshGeneralList();
            HideDetail();
        }

        /// <summary>
        /// 刷新將領列表
        /// </summary>
        public void RefreshGeneralList()
        {
            UIHelper.ClearChildren(_generalListContainer);
            _listItems.Clear();

            var generals = GeneralManager.Instance.GetAllGenerals();

            if (_generalCountText != null)
                _generalCountText.text = $"將領數量: {generals.Count}";

            foreach (var general in generals)
            {
                CreateGeneralItem(general);
            }
        }

        private void CreateGeneralItem(GeneralData general)
        {
            if (_generalItemPrefab == null || _generalListContainer == null) return;

            var item = Instantiate(_generalItemPrefab, _generalListContainer);
            item.Setup(general, OnGeneralSelected);
            _listItems.Add(item);
        }

        private void OnGeneralSelected(GeneralData general)
        {
            _selectedGeneral = general;
            ShowDetail(general);
        }

        private void ShowDetail(GeneralData general)
        {
            if (_detailPanel != null)
                _detailPanel.SetActive(true);

            if (_generalNameText != null)
                _generalNameText.text = general.Name;

            if (_generalClassText != null)
                _generalClassText.text = GetClassName(general.Class);

            if (_generalLevelText != null)
                _generalLevelText.text = $"Lv.{general.Level}";

            // 經驗值
            int expNeeded = GeneralManager.Instance.GetExpForNextLevel(general.Level);
            if (_generalExpText != null)
                _generalExpText.text = $"{general.Exp}/{expNeeded}";

            if (_expProgressBar != null)
                _expProgressBar.fillAmount = (float)general.Exp / expNeeded;

            // 屬性
            if (_strengthText != null)
                _strengthText.text = $"武力: {general.Strength:F1}";

            if (_intelligenceText != null)
                _intelligenceText.text = $"智力: {general.Intelligence:F1}";

            if (_commandText != null)
                _commandText.text = $"統帥: {general.Command:F1}";

            if (_speedText != null)
                _speedText.text = $"速度: {general.Speed:F1}";

            if (_maxTroopsText != null)
                _maxTroopsText.text = $"帶兵上限: {general.GetMaxTroops()}";

            if (_powerText != null)
                _powerText.text = $"戰力: {GeneralManager.Instance.CalculatePower(general)}";

            // 稀有度星星
            RefreshStars(general.Rarity);
        }

        private void RefreshStars(int rarity)
        {
            if (_starContainer == null || _starPrefab == null) return;

            UIHelper.ClearChildren(_starContainer);

            for (int i = 0; i < rarity; i++)
            {
                Instantiate(_starPrefab, _starContainer);
            }
        }

        private void HideDetail()
        {
            if (_detailPanel != null)
                _detailPanel.SetActive(false);
            _selectedGeneral = null;
        }

        private string GetClassName(GeneralClass generalClass)
        {
            return generalClass switch
            {
                GeneralClass.Commander => "統帥",
                GeneralClass.Vanguard => "先鋒",
                GeneralClass.Strategist => "軍師",
                _ => "未知"
            };
        }

        #region 按鈕事件

        private void OnRecruitClicked()
        {
            // 測試用: 隨機招募一個將領
            var classes = new[] { GeneralClass.Commander, GeneralClass.Vanguard, GeneralClass.Strategist };
            var names = new[] { "張遼", "趙雲", "諸葛亮", "關羽", "呂布", "周瑜", "司馬懿" };

            int rarity = Random.Range(1, 6);
            var generalClass = classes[Random.Range(0, classes.Length)];
            string name = names[Random.Range(0, names.Length)] + Random.Range(1, 100);

            GeneralManager.Instance.AddGeneral(
                name,
                rarity,
                generalClass,
                Random.Range(50f, 100f),
                Random.Range(50f, 100f),
                Random.Range(50f, 100f),
                Random.Range(50f, 100f)
            );

            RefreshGeneralList();
        }

        private void OnDismissClicked()
        {
            if (_selectedGeneral == null) return;

            GeneralManager.Instance.DismissGeneral(_selectedGeneral.GeneralId);
            HideDetail();
            RefreshGeneralList();
        }

        #endregion
    }
}
