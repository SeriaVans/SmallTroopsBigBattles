using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 將領列表項目 UI
    /// </summary>
    public class GeneralListItemUI : MonoBehaviour
    {
        [Header("UI 元件")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _classText;
        [SerializeField] private Transform _starContainer;
        [SerializeField] private Image _starPrefab;
        [SerializeField] private Image _frameImage;

        [Header("稀有度顏色")]
        [SerializeField] private Color _rarity1Color = Color.gray;
        [SerializeField] private Color _rarity2Color = Color.green;
        [SerializeField] private Color _rarity3Color = Color.blue;
        [SerializeField] private Color _rarity4Color = new Color(0.5f, 0, 0.5f); // 紫色
        [SerializeField] private Color _rarity5Color = new Color(1f, 0.8f, 0); // 金色

        private GeneralData _general;
        private Action<GeneralData> _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void Setup(GeneralData general, Action<GeneralData> onClick)
        {
            _general = general;
            _onClick = onClick;

            Refresh();
        }

        public void Refresh()
        {
            if (_general == null) return;

            if (_nameText != null)
                _nameText.text = _general.Name;

            if (_levelText != null)
                _levelText.text = $"Lv.{_general.Level}";

            if (_classText != null)
                _classText.text = GetClassName(_general.Class);

            // 稀有度邊框顏色
            if (_frameImage != null)
                _frameImage.color = GetRarityColor(_general.Rarity);

            // 星星顯示
            RefreshStars();
        }

        private void RefreshStars()
        {
            if (_starContainer == null || _starPrefab == null) return;

            UIHelper.ClearChildren(_starContainer);

            for (int i = 0; i < _general.Rarity; i++)
            {
                var star = Instantiate(_starPrefab, _starContainer);
                star.color = GetRarityColor(_general.Rarity);
            }
        }

        private Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                1 => _rarity1Color,
                2 => _rarity2Color,
                3 => _rarity3Color,
                4 => _rarity4Color,
                5 => _rarity5Color,
                _ => Color.white
            };
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

        private void OnClicked()
        {
            _onClick?.Invoke(_general);
        }
    }
}
