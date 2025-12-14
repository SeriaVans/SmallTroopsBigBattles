using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Managers;

namespace SmallTroopsBigBattles.UI
{
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
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private TextMeshProUGUI _intelligenceText;
        [SerializeField] private TextMeshProUGUI _commandText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _powerText;

        [Header("操作按鈕")]
        [SerializeField] private Button _recruitButton;
        [SerializeField] private Button _dismissButton;

        private GeneralData _selectedGeneral;

        protected override void Awake()
        {
            base.Awake();
            if (_recruitButton) _recruitButton.onClick.AddListener(OnRecruitClicked);
            if (_dismissButton) _dismissButton.onClick.AddListener(OnDismissClicked);
        }

        protected override void OnPanelOpened() { RefreshGeneralList(); HideDetail(); }

        public void RefreshGeneralList()
        {
            UIHelper.ClearChildren(_generalListContainer);
            var generals = GeneralManager.Instance.GetAllGenerals();
            if (_generalCountText) _generalCountText.text = $"將領: {generals.Count}";

            foreach (var g in generals)
            {
                if (_generalItemPrefab && _generalListContainer)
                {
                    var item = Instantiate(_generalItemPrefab, _generalListContainer);
                    item.Setup(g, OnGeneralSelected);
                }
            }
        }

        private void OnGeneralSelected(GeneralData general) { _selectedGeneral = general; ShowDetail(general); }

        private void ShowDetail(GeneralData g)
        {
            if (_detailPanel) _detailPanel.SetActive(true);
            if (_generalNameText) _generalNameText.text = g.Name;
            if (_generalClassText) _generalClassText.text = g.Class switch { GeneralClass.Commander => "統帥", GeneralClass.Vanguard => "先鋒", GeneralClass.Strategist => "軍師", _ => "?" };
            if (_generalLevelText) _generalLevelText.text = $"Lv.{g.Level}";
            if (_strengthText) _strengthText.text = $"武力: {g.Strength:F1}";
            if (_intelligenceText) _intelligenceText.text = $"智力: {g.Intelligence:F1}";
            if (_commandText) _commandText.text = $"統帥: {g.Command:F1}";
            if (_speedText) _speedText.text = $"速度: {g.Speed:F1}";
            if (_powerText) _powerText.text = $"戰力: {GeneralManager.Instance.CalculatePower(g)}";
        }

        private void HideDetail() { if (_detailPanel) _detailPanel.SetActive(false); _selectedGeneral = null; }

        private void OnRecruitClicked()
        {
            var classes = new[] { GeneralClass.Commander, GeneralClass.Vanguard, GeneralClass.Strategist };
            var names = new[] { "張遼", "趙雲", "諸葛亮", "關羽", "呂布", "周瑜" };
            GeneralManager.Instance.AddGeneral(
                names[Random.Range(0, names.Length)] + Random.Range(1, 100),
                Random.Range(1, 6),
                classes[Random.Range(0, classes.Length)],
                Random.Range(50f, 100f), Random.Range(50f, 100f), Random.Range(50f, 100f), Random.Range(50f, 100f)
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
    }
}
