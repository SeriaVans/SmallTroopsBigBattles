using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;

namespace SmallTroopsBigBattles.UI
{
    public class GeneralListItemUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _classText;
        [SerializeField] private Image _frameImage;

        [Header("稀有度顏色")]
        [SerializeField] private Color _rarity1Color = Color.gray;
        [SerializeField] private Color _rarity2Color = Color.green;
        [SerializeField] private Color _rarity3Color = Color.blue;
        [SerializeField] private Color _rarity4Color = new Color(0.5f, 0, 0.5f);
        [SerializeField] private Color _rarity5Color = new Color(1f, 0.8f, 0);

        private GeneralData _general;
        private Action<GeneralData> _onClick;

        private void Awake() { if (_button) _button.onClick.AddListener(() => _onClick?.Invoke(_general)); }

        public void Setup(GeneralData general, Action<GeneralData> onClick)
        {
            _general = general;
            _onClick = onClick;
            Refresh();
        }

        public void Refresh()
        {
            if (_general == null) return;
            if (_nameText) _nameText.text = _general.Name;
            if (_levelText) _levelText.text = $"Lv.{_general.Level}";
            if (_classText) _classText.text = _general.Class switch { GeneralClass.Commander => "統帥", GeneralClass.Vanguard => "先鋒", GeneralClass.Strategist => "軍師", _ => "?" };
            if (_frameImage) _frameImage.color = GetRarityColor(_general.Rarity);
        }

        private Color GetRarityColor(int r) => r switch { 1 => _rarity1Color, 2 => _rarity2Color, 3 => _rarity3Color, 4 => _rarity4Color, 5 => _rarity5Color, _ => Color.white };
    }
}
