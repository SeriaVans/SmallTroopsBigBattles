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
    /// 領地面板 - 顯示領地資訊與建築管理
    /// </summary>
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
        [SerializeField] private TextMeshProUGUI _territoryLevelText;
        [SerializeField] private TextMeshProUGUI _buildingCountText;

        [Header("建築詳情彈窗")]
        [SerializeField] private GameObject _buildingDetailPopup;
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private TextMeshProUGUI _buildingLevelText;
        [SerializeField] private TextMeshProUGUI _buildingDescText;
        [SerializeField] private TextMeshProUGUI _buildingCostText;
        [SerializeField] private Button _buildButton;
        [SerializeField] private Button _upgradeButton;

        private TerritoryData _currentTerritory;
        private int _selectedSlotIndex = -1;
        private List<BuildingSlotUI> _slotUIs = new List<BuildingSlotUI>();

        protected override void OnPanelOpened()
        {
            LoadTerritories();
            HideBuildingDetail();
        }

        private void LoadTerritories()
        {
            var territories = TerritoryManager.Instance.GetPlayerTerritories();

            // 清空現有 Tab
            UIHelper.ClearChildren(_territoryTabContainer);

            if (territories.Count == 0)
            {
                // 建立測試領地
                CreateTestTerritory();
                territories = TerritoryManager.Instance.GetPlayerTerritories();
            }

            // 建立 Tab
            foreach (var territory in territories)
            {
                CreateTerritoryTab(territory);
            }

            // 選擇第一個領地
            if (territories.Count > 0)
            {
                SelectTerritory(territories[0]);
            }
        }

        private void CreateTestTerritory()
        {
            var territory = new TerritoryData
            {
                TerritoryId = 1,
                PlayerId = GameManager.Instance.CurrentPlayer.PlayerId,
                CityId = 1,
                TerritoryLevel = 1
            };
            TerritoryManager.Instance.AddTerritory(territory);
        }

        private void CreateTerritoryTab(TerritoryData territory)
        {
            if (_territoryTabPrefab == null || _territoryTabContainer == null) return;

            var tab = Instantiate(_territoryTabPrefab, _territoryTabContainer);
            var tabText = tab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.text = $"領地 {territory.TerritoryId}";
            }

            tab.onClick.AddListener(() => SelectTerritory(territory));
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

            if (_territoryNameText != null)
                _territoryNameText.text = $"領地 {_currentTerritory.TerritoryId}";

            if (_territoryLevelText != null)
                _territoryLevelText.text = $"等級: {_currentTerritory.TerritoryLevel}";

            int maxSlots = TerritoryManager.Instance.GetMaxBuildingSlots(_currentTerritory);
            if (_buildingCountText != null)
                _buildingCountText.text = $"建築: {_currentTerritory.Buildings.Count}/{maxSlots}";
        }

        private void RefreshBuildingSlots()
        {
            if (_currentTerritory == null) return;

            // 清空現有格子
            UIHelper.ClearChildren(_buildingSlotContainer);
            _slotUIs.Clear();

            int maxSlots = TerritoryManager.Instance.GetMaxBuildingSlots(_currentTerritory);

            for (int i = 0; i < maxSlots; i++)
            {
                var building = _currentTerritory.Buildings.Find(b => b.SlotIndex == i);
                CreateBuildingSlot(i, building);
            }
        }

        private void CreateBuildingSlot(int slotIndex, BuildingData building)
        {
            if (_buildingSlotPrefab == null || _buildingSlotContainer == null) return;

            var slotUI = Instantiate(_buildingSlotPrefab, _buildingSlotContainer);
            slotUI.Setup(slotIndex, building, OnSlotClicked);
            _slotUIs.Add(slotUI);
        }

        private void OnSlotClicked(int slotIndex, BuildingData building)
        {
            _selectedSlotIndex = slotIndex;
            ShowBuildingDetail(slotIndex, building);
        }

        private void ShowBuildingDetail(int slotIndex, BuildingData building)
        {
            if (_buildingDetailPopup == null) return;

            _buildingDetailPopup.SetActive(true);

            if (building == null)
            {
                // 空格 - 顯示建造選項
                if (_buildingNameText != null)
                    _buildingNameText.text = "空建築格";
                if (_buildingDescText != null)
                    _buildingDescText.text = "點擊建造按鈕選擇要建造的建築";
                if (_buildingLevelText != null)
                    _buildingLevelText.text = "";
                if (_buildingCostText != null)
                    _buildingCostText.text = "";

                UIHelper.SetActive(_buildButton, true);
                UIHelper.SetActive(_upgradeButton, false);

                if (_buildButton != null)
                {
                    _buildButton.onClick.RemoveAllListeners();
                    _buildButton.onClick.AddListener(() => ShowBuildingTypeSelection(slotIndex));
                }
            }
            else
            {
                // 有建築 - 顯示升級選項
                if (_buildingNameText != null)
                    _buildingNameText.text = GetBuildingName(building.Type);
                if (_buildingLevelText != null)
                    _buildingLevelText.text = $"等級: {building.Level}";
                if (_buildingDescText != null)
                    _buildingDescText.text = building.IsConstructing ? "建造中..." : GetBuildingDesc(building.Type);

                var costs = TerritoryManager.Instance.GetBuildingCost(building.Type, building.Level + 1);
                if (_buildingCostText != null)
                    _buildingCostText.text = FormatCosts(costs);

                UIHelper.SetActive(_buildButton, false);
                UIHelper.SetActive(_upgradeButton, !building.IsConstructing);

                if (_upgradeButton != null)
                {
                    _upgradeButton.onClick.RemoveAllListeners();
                    _upgradeButton.onClick.AddListener(() => UpgradeBuilding(slotIndex));
                }
            }
        }

        private void HideBuildingDetail()
        {
            if (_buildingDetailPopup != null)
                _buildingDetailPopup.SetActive(false);
        }

        private void ShowBuildingTypeSelection(int slotIndex)
        {
            // TODO: 顯示建築類型選擇面板
            // 暫時直接建造農田
            TerritoryManager.Instance.ConstructBuilding(
                _currentTerritory.TerritoryId,
                slotIndex,
                TerritoryBuildingType.Farm
            );

            RefreshBuildingSlots();
            HideBuildingDetail();
        }

        private void UpgradeBuilding(int slotIndex)
        {
            if (_currentTerritory == null) return;

            if (TerritoryManager.Instance.UpgradeBuilding(_currentTerritory.TerritoryId, slotIndex))
            {
                RefreshBuildingSlots();
                HideBuildingDetail();
            }
        }

        private string GetBuildingName(TerritoryBuildingType type)
        {
            return type switch
            {
                TerritoryBuildingType.Farm => "農田",
                TerritoryBuildingType.Lumberyard => "伐木場",
                TerritoryBuildingType.Quarry => "採石場",
                TerritoryBuildingType.Barracks => "兵營",
                TerritoryBuildingType.Market => "市場",
                TerritoryBuildingType.Warehouse => "倉庫",
                TerritoryBuildingType.Academy => "學院",
                _ => "未知建築"
            };
        }

        private string GetBuildingDesc(TerritoryBuildingType type)
        {
            return type switch
            {
                TerritoryBuildingType.Farm => "生產糧草",
                TerritoryBuildingType.Lumberyard => "生產木材",
                TerritoryBuildingType.Quarry => "生產石材",
                TerritoryBuildingType.Barracks => "訓練士兵",
                TerritoryBuildingType.Market => "生產銅錢",
                TerritoryBuildingType.Warehouse => "增加資源上限",
                TerritoryBuildingType.Academy => "研究科技",
                _ => ""
            };
        }

        private string FormatCosts(Dictionary<ResourceType, int> costs)
        {
            var parts = new List<string>();
            foreach (var cost in costs)
            {
                string name = cost.Key switch
                {
                    ResourceType.Copper => "銅",
                    ResourceType.Wood => "木",
                    ResourceType.Stone => "石",
                    ResourceType.Food => "糧",
                    _ => ""
                };
                parts.Add($"{name}:{cost.Value}");
            }
            return string.Join(" ", parts);
        }
    }
}
