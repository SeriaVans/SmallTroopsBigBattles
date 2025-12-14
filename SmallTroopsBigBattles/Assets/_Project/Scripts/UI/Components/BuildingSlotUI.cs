using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;

namespace SmallTroopsBigBattles.UI
{
    public class BuildingSlotUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _emptyIndicator;
        [SerializeField] private GameObject _constructingIndicator;
        [SerializeField] private TextMeshProUGUI _constructingTimeText;

        private int _slotIndex;
        private BuildingData _building;
        private Action<int, BuildingData> _onClick;

        private void Awake() { if (_button) _button.onClick.AddListener(() => _onClick?.Invoke(_slotIndex, _building)); }

        public void Setup(int slotIndex, BuildingData building, Action<int, BuildingData> onClick)
        {
            _slotIndex = slotIndex;
            _building = building;
            _onClick = onClick;
            Refresh();
        }

        public void Refresh()
        {
            bool isEmpty = _building == null;
            bool isConstructing = _building?.IsConstructing ?? false;
            UIHelper.SetActive(_emptyIndicator, isEmpty);
            UIHelper.SetActive(_constructingIndicator, isConstructing);
            if (_levelText) _levelText.text = isEmpty ? "" : $"Lv.{_building.Level}";
            if (_buildingIcon) _buildingIcon.gameObject.SetActive(!isEmpty);
        }

        private void Update()
        {
            if (_building != null && _building.IsConstructing && _constructingTimeText)
            {
                var remaining = (_building.ConstructionEndTime - DateTime.Now).TotalSeconds;
                _constructingTimeText.text = remaining > 0 ? Core.Utils.TimeUtils.FormatTimeShort((float)remaining) : "完成";
            }
        }
    }
}
