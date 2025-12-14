using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 建築格 UI 元件
    /// </summary>
    public class BuildingSlotUI : MonoBehaviour
    {
        [Header("UI 元件")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _emptyIndicator;
        [SerializeField] private GameObject _constructingIndicator;
        [SerializeField] private TextMeshProUGUI _constructingTimeText;

        private int _slotIndex;
        private BuildingData _building;
        private Action<int, BuildingData> _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

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

            if (isEmpty)
            {
                if (_levelText != null)
                    _levelText.text = "";
                if (_buildingIcon != null)
                    _buildingIcon.gameObject.SetActive(false);
            }
            else
            {
                if (_levelText != null)
                    _levelText.text = $"Lv.{_building.Level}";
                if (_buildingIcon != null)
                {
                    _buildingIcon.gameObject.SetActive(true);
                    // TODO: 設置建築圖示
                }
            }
        }

        private void Update()
        {
            if (_building != null && _building.IsConstructing)
            {
                UpdateConstructionTime();
            }
        }

        private void UpdateConstructionTime()
        {
            if (_constructingTimeText == null) return;

            var remaining = (_building.ConstructionEndTime - DateTime.Now).TotalSeconds;
            if (remaining > 0)
            {
                _constructingTimeText.text = Core.Utils.TimeUtils.FormatTimeShort((float)remaining);
            }
            else
            {
                _constructingTimeText.text = "完成";
            }
        }

        private void OnClicked()
        {
            _onClick?.Invoke(_slotIndex, _building);
        }
    }
}
