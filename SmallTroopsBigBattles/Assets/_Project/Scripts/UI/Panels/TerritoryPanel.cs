using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;
using SmallTroopsBigBattles.Game.Territory;

namespace SmallTroopsBigBattles.UI.Panels
{
    /// <summary>
    /// 領地面板
    /// </summary>
    public class TerritoryPanel : BasePanel
    {
        [Header("領地標籤")]
        [SerializeField] private Transform territoryTabContainer;
        [SerializeField] private Button territoryTabPrefab;

        [Header("建築格")]
        [SerializeField] private Transform buildingSlotContainer;
        [SerializeField] private Button buildingSlotPrefab;

        [Header("領地資訊")]
        [SerializeField] private TextMeshProUGUI territoryNameText;
        [SerializeField] private TextMeshProUGUI territoryLevelText;
        [SerializeField] private TextMeshProUGUI buildingCountText;

        [Header("建築詳情彈窗")]
        [SerializeField] private GameObject buildingDetailPopup;
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI buildingLevelText;
        [SerializeField] private TextMeshProUGUI buildingDescText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button upgradeButton;

        private List<Button> _territoryTabs = new();
        private List<Button> _buildingSlots = new();
        private int _selectedSlotIndex = -1;

        protected override void Awake()
        {
            base.Awake();

            if (buildButton != null)
                buildButton.onClick.AddListener(OnBuildButtonClick);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeButtonClick);

            // 訂閱事件
            EventManager.Instance?.Subscribe<TerritorySelectedEvent>(OnTerritorySelected);
            EventManager.Instance?.Subscribe<BuildingConstructedEvent>(OnBuildingConstructed);
            EventManager.Instance?.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
        }

        protected override void OnDestroy()
        {
            // 取消訂閱
            EventManager.Instance?.Unsubscribe<TerritorySelectedEvent>(OnTerritorySelected);
            EventManager.Instance?.Unsubscribe<BuildingConstructedEvent>(OnBuildingConstructed);
            EventManager.Instance?.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);

            base.OnDestroy();
        }

        protected override void OnShow()
        {
            RefreshData();
            HideBuildingDetail();
        }

        public override void RefreshData()
        {
            RefreshTerritoryTabs();
            RefreshBuildingSlots();
            RefreshTerritoryInfo();
        }

        /// <summary>
        /// 刷新領地標籤
        /// </summary>
        private void RefreshTerritoryTabs()
        {
            if (territoryTabContainer == null || territoryTabPrefab == null) return;

            // 清除現有標籤
            foreach (var tab in _territoryTabs)
            {
                Destroy(tab.gameObject);
            }
            _territoryTabs.Clear();

            // 創建新標籤
            var territories = TerritoryManager.Instance?.PlayerTerritories;
            if (territories == null) return;

            for (int i = 0; i < territories.TerritoryCount; i++)
            {
                var tab = Instantiate(territoryTabPrefab, territoryTabContainer);
                var territory = territories.Territories[i];

                // 設置標籤文字
                var tabText = tab.GetComponentInChildren<TextMeshProUGUI>();
                if (tabText != null)
                {
                    tabText.text = $"領地 {territory.TerritoryId}";
                }

                // 綁定點擊事件
                int index = i;
                tab.onClick.AddListener(() => OnTerritoryTabClick(index));

                _territoryTabs.Add(tab);
            }

            // 高亮當前選中的標籤
            UpdateTabHighlight();
        }

        /// <summary>
        /// 刷新建築格
        /// </summary>
        private void RefreshBuildingSlots()
        {
            if (buildingSlotContainer == null || buildingSlotPrefab == null) return;

            // 清除現有建築格
            foreach (var slot in _buildingSlots)
            {
                Destroy(slot.gameObject);
            }
            _buildingSlots.Clear();

            var territory = TerritoryManager.Instance?.CurrentTerritory;
            if (territory == null) return;

            // 創建建築格
            for (int i = 0; i < territory.AvailableBuildingSlots; i++)
            {
                var slot = Instantiate(buildingSlotPrefab, buildingSlotContainer);
                var building = territory.GetBuildingAt(i);

                // 設置建築格外觀
                var slotText = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (slotText != null)
                {
                    if (building != null)
                    {
                        slotText.text = $"{TerritoryManager.GetBuildingDisplayName(building.Type)}\nLv{building.Level}";
                    }
                    else
                    {
                        slotText.text = "+";
                    }
                }

                // 綁定點擊事件
                int index = i;
                slot.onClick.AddListener(() => OnBuildingSlotClick(index));

                _buildingSlots.Add(slot);
            }
        }

        /// <summary>
        /// 刷新領地資訊
        /// </summary>
        private void RefreshTerritoryInfo()
        {
            var territory = TerritoryManager.Instance?.CurrentTerritory;
            if (territory == null) return;

            if (territoryNameText != null)
                territoryNameText.text = $"領地 {territory.TerritoryId}";

            if (territoryLevelText != null)
                territoryLevelText.text = $"等級: {territory.TerritoryLevel}";

            if (buildingCountText != null)
                buildingCountText.text = $"建築: {territory.UsedBuildingSlots}/{territory.AvailableBuildingSlots}";
        }

        /// <summary>
        /// 更新標籤高亮
        /// </summary>
        private void UpdateTabHighlight()
        {
            int currentIndex = TerritoryManager.Instance?.CurrentTerritoryIndex ?? 0;
            for (int i = 0; i < _territoryTabs.Count; i++)
            {
                var colors = _territoryTabs[i].colors;
                colors.normalColor = i == currentIndex ? Color.green : Color.white;
                _territoryTabs[i].colors = colors;
            }
        }

        /// <summary>
        /// 領地標籤點擊
        /// </summary>
        private void OnTerritoryTabClick(int index)
        {
            TerritoryManager.Instance?.SelectTerritory(index);
        }

        /// <summary>
        /// 建築格點擊
        /// </summary>
        private void OnBuildingSlotClick(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;
            ShowBuildingDetail(slotIndex);
        }

        /// <summary>
        /// 顯示建築詳情
        /// </summary>
        private void ShowBuildingDetail(int slotIndex)
        {
            if (buildingDetailPopup == null) return;

            var territory = TerritoryManager.Instance?.CurrentTerritory;
            if (territory == null) return;

            var building = territory.GetBuildingAt(slotIndex);

            if (building != null)
            {
                // 已有建築 - 顯示升級選項
                if (buildingNameText != null)
                    buildingNameText.text = TerritoryManager.GetBuildingDisplayName(building.Type);

                if (buildingLevelText != null)
                    buildingLevelText.text = $"等級: {building.Level}";

                if (buildingDescText != null)
                    buildingDescText.text = TerritoryManager.GetBuildingDescription(building.Type);

                var cost = TerritoryManager.Instance.GetBuildingCost(building.Type, building.Level + 1);
                if (costText != null)
                    costText.text = $"升級消耗: 銅{cost.copper} 木{cost.wood} 石{cost.stone} 糧{cost.food}";

                if (buildButton != null) buildButton.gameObject.SetActive(false);
                if (upgradeButton != null)
                {
                    upgradeButton.gameObject.SetActive(true);
                    upgradeButton.interactable = building.CanUpgrade;
                }
            }
            else
            {
                // 空格 - 顯示建造選項
                if (buildingNameText != null)
                    buildingNameText.text = "空地";

                if (buildingLevelText != null)
                    buildingLevelText.text = "";

                if (buildingDescText != null)
                    buildingDescText.text = "可建造新建築";

                if (costText != null)
                    costText.text = "選擇要建造的建築類型";

                if (buildButton != null) buildButton.gameObject.SetActive(true);
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            }

            buildingDetailPopup.SetActive(true);
        }

        /// <summary>
        /// 隱藏建築詳情
        /// </summary>
        private void HideBuildingDetail()
        {
            if (buildingDetailPopup != null)
                buildingDetailPopup.SetActive(false);
        }

        /// <summary>
        /// 建造按鈕點擊
        /// </summary>
        private void OnBuildButtonClick()
        {
            // TODO: 顯示建築類型選擇介面
            // 暫時直接建造農場
            if (_selectedSlotIndex >= 0)
            {
                TerritoryManager.Instance?.BuildBuilding(_selectedSlotIndex, BuildingType.Farm);
            }
        }

        /// <summary>
        /// 升級按鈕點擊
        /// </summary>
        private void OnUpgradeButtonClick()
        {
            if (_selectedSlotIndex >= 0)
            {
                TerritoryManager.Instance?.UpgradeBuilding(_selectedSlotIndex);
            }
        }

        // 事件處理
        private void OnTerritorySelected(TerritorySelectedEvent evt)
        {
            RefreshData();
        }

        private void OnBuildingConstructed(BuildingConstructedEvent evt)
        {
            RefreshBuildingSlots();
            RefreshTerritoryInfo();
            HideBuildingDetail();
        }

        private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
        {
            RefreshBuildingSlots();
            ShowBuildingDetail(_selectedSlotIndex);
        }
    }
}

