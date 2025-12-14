using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Managers;

namespace SmallTroopsBigBattles.UI
{
    public class TerritoryPanel : BasePanel
    {
        [Header("領地選擇")]
        [SerializeField] private Transform _territoryTabContainer;
        [SerializeField] private Button _territoryTabPrefab;

        [Header("建築格顯示")]
        [SerializeField] private Transform _buildingSlotContainer;
        [SerializeField] private BuildingSlotUI _buildingSlotPrefab;

        [Header("資訊顯示")]
        [SerializeField] private TextMeshProUGUI _territoryNameText;
        [SerializeField] private TextMeshProUGUI _buildingCountText;

        [Header("建築詳情")]
        [SerializeField] private GameObject _buildingDetailPopup;
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private TextMeshProUGUI _buildingLevelText;
        [SerializeField] private Button _buildButton;
        [SerializeField] private Button _upgradeButton;

        private TerritoryData _currentTerritory;
        private int _selectedSlotIndex = -1;

        protected override void OnPanelOpened()
        {
            LoadTerritories();
            HideBuildingDetail();
        }

        private void LoadTerritories()
        {
            var territories = TerritoryManager.Instance.GetPlayerTerritories();
            UIHelper.ClearChildren(_territoryTabContainer);

            if (territories.Count == 0)
            {
                var t = new TerritoryData { TerritoryId = 1, PlayerId = GameManager.Instance.CurrentPlayer.PlayerId, TerritoryLevel = 1 };
                TerritoryManager.Instance.AddTerritory(t);
                territories = TerritoryManager.Instance.GetPlayerTerritories();
            }

            foreach (var territory in territories)
            {
                if (_territoryTabPrefab && _territoryTabContainer)
                {
                    var tab = Instantiate(_territoryTabPrefab, _territoryTabContainer);
                    var txt = tab.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt) txt.text = $"領地 {territory.TerritoryId}";
                    tab.onClick.AddListener(() => SelectTerritory(territory));
                }
            }
            if (territories.Count > 0) SelectTerritory(territories[0]);
        }

        private void SelectTerritory(TerritoryData territory)
        {
            _currentTerritory = territory;
            RefreshTerritoryInfo();
            RefreshBuildingSlots();
        }

        private void RefreshTerritoryInfo()
        {
            if (_currentTerritory == null) return;
            if (_territoryNameText) _territoryNameText.text = $"領地 {_currentTerritory.TerritoryId}";
            int maxSlots = TerritoryManager.Instance.GetMaxBuildingSlots(_currentTerritory);
            if (_buildingCountText) _buildingCountText.text = $"建築: {_currentTerritory.Buildings.Count}/{maxSlots}";
        }

        private void RefreshBuildingSlots()
        {
            if (_currentTerritory == null) return;
            UIHelper.ClearChildren(_buildingSlotContainer);
            int maxSlots = TerritoryManager.Instance.GetMaxBuildingSlots(_currentTerritory);
            for (int i = 0; i < maxSlots; i++)
            {
                var building = _currentTerritory.Buildings.Find(b => b.SlotIndex == i);
                if (_buildingSlotPrefab && _buildingSlotContainer)
                {
                    var slot = Instantiate(_buildingSlotPrefab, _buildingSlotContainer);
                    slot.Setup(i, building, OnSlotClicked);
                }
            }
        }

        private void OnSlotClicked(int slotIndex, BuildingData building)
        {
            _selectedSlotIndex = slotIndex;
            ShowBuildingDetail(slotIndex, building);
        }

        private void ShowBuildingDetail(int slotIndex, BuildingData building)
        {
            if (_buildingDetailPopup) _buildingDetailPopup.SetActive(true);

            if (building == null)
            {
                if (_buildingNameText) _buildingNameText.text = "空建築格";
                if (_buildingLevelText) _buildingLevelText.text = "";
                UIHelper.SetActive(_buildButton, true);
                UIHelper.SetActive(_upgradeButton, false);
                if (_buildButton)
                {
                    _buildButton.onClick.RemoveAllListeners();
                    _buildButton.onClick.AddListener(() => {
                        TerritoryManager.Instance.ConstructBuilding(_currentTerritory.TerritoryId, slotIndex, TerritoryBuildingType.Farm);
                        RefreshBuildingSlots();
                        HideBuildingDetail();
                    });
                }
            }
            else
            {
                if (_buildingNameText) _buildingNameText.text = GetBuildingName(building.Type);
                if (_buildingLevelText) _buildingLevelText.text = building.IsConstructing ? "建造中..." : $"Lv.{building.Level}";
                UIHelper.SetActive(_buildButton, false);
                UIHelper.SetActive(_upgradeButton, !building.IsConstructing);
                if (_upgradeButton)
                {
                    _upgradeButton.onClick.RemoveAllListeners();
                    _upgradeButton.onClick.AddListener(() => {
                        TerritoryManager.Instance.UpgradeBuilding(_currentTerritory.TerritoryId, slotIndex);
                        RefreshBuildingSlots();
                        HideBuildingDetail();
                    });
                }
            }
        }

        private void HideBuildingDetail() { if (_buildingDetailPopup) _buildingDetailPopup.SetActive(false); }

        private string GetBuildingName(TerritoryBuildingType type) => type switch
        {
            TerritoryBuildingType.Farm => "農田",
            TerritoryBuildingType.Lumberyard => "伐木場",
            TerritoryBuildingType.Quarry => "採石場",
            TerritoryBuildingType.Barracks => "兵營",
            TerritoryBuildingType.Market => "市場",
            TerritoryBuildingType.Warehouse => "倉庫",
            TerritoryBuildingType.Academy => "學院",
            _ => "未知"
        };
    }
}
