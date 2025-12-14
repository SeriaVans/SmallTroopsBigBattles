using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Resource;
using SmallTroopsBigBattles.Game.Army;
using SmallTroopsBigBattles.UI.Panels;
using SmallTroopsBigBattles.Game.Battle;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 遊戲主 HUD - 顯示資源、功能按鈕等
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("玩家資訊")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerLevelText;

        [Header("資源顯示")]
        [SerializeField] private TextMeshProUGUI copperText;
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI stoneText;
        [SerializeField] private TextMeshProUGUI foodText;

        [Header("軍隊顯示")]
        [SerializeField] private TextMeshProUGUI soldierCountText;

        [Header("功能按鈕")]
        [SerializeField] private Button territoryButton;
        [SerializeField] private Button armyButton;
        [SerializeField] private Button generalButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button settingsButton;

        private void Awake()
        {
            // 自動查找並連接 UI 元素（如果引用為空）
            AutoConnectReferences();

            // 綁定按鈕事件
            if (territoryButton != null)
                territoryButton.onClick.AddListener(OnTerritoryButtonClick);

            if (armyButton != null)
                armyButton.onClick.AddListener(OnArmyButtonClick);

            if (generalButton != null)
                generalButton.onClick.AddListener(OnGeneralButtonClick);

            if (mapButton != null)
                mapButton.onClick.AddListener(OnMapButtonClick);

            if (questButton != null)
                questButton.onClick.AddListener(OnQuestButtonClick);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }

        /// <summary>
        /// 自動連接 UI 引用（如果為空）
        /// </summary>
        private void AutoConnectReferences()
        {
            // 玩家資訊
            if (playerNameText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/PlayerInfoPanel/PlayerNameText");
                if (obj != null) playerNameText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (playerLevelText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/PlayerInfoPanel/PlayerLevelText");
                if (obj != null) playerLevelText = obj.GetComponent<TextMeshProUGUI>();
            }

            // 資源顯示
            if (copperText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/TopResourceBar/CopperText");
                if (obj != null) copperText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (woodText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/TopResourceBar/WoodText");
                if (obj != null) woodText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (stoneText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/TopResourceBar/StoneText");
                if (obj != null) stoneText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (foodText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/TopResourceBar/FoodText");
                if (obj != null) foodText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (soldierCountText == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/TopResourceBar/SoldierCountText");
                if (obj != null) soldierCountText = obj.GetComponent<TextMeshProUGUI>();
            }

            // 按鈕
            if (territoryButton == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/TerritoryButton");
                if (obj != null) territoryButton = obj.GetComponent<Button>();
            }

            if (armyButton == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/ArmyButton");
                if (obj != null) armyButton = obj.GetComponent<Button>();
            }

            if (generalButton == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/GeneralButton");
                if (obj != null) generalButton = obj.GetComponent<Button>();
            }

            if (mapButton == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/MapButton");
                if (obj != null) mapButton = obj.GetComponent<Button>();
            }

            if (settingsButton == null)
            {
                var obj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/SettingsButton");
                if (obj != null) settingsButton = obj.GetComponent<Button>();
            }

            // 測試戰鬥按鈕（如果存在）
            var testBattleButtonObj = GameObject.Find("MainCanvas/HUD/BottomButtonBar/TestBattleButton");
            if (testBattleButtonObj != null)
            {
                var testBattleButton = testBattleButtonObj.GetComponent<Button>();
                if (testBattleButton != null)
                {
                    testBattleButton.onClick.AddListener(OnTestBattleButtonClick);
                }
            }
        }

        private void Start()
        {
            // 訂閱事件
            EventManager.Instance?.Subscribe<ResourceChangedEvent>(OnResourceChanged);
            EventManager.Instance?.Subscribe<SoldiersChangedEvent>(OnSoldiersChanged);
            EventManager.Instance?.Subscribe<GameInitializedEvent>(OnGameInitialized);

            // 如果遊戲已經初始化，立即刷新
            if (GameManager.HasInstance && GameManager.Instance.IsInitialized)
            {
                RefreshAll();
            }
        }

        private void OnDestroy()
        {
            EventManager.Instance?.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
            EventManager.Instance?.Unsubscribe<SoldiersChangedEvent>(OnSoldiersChanged);
            EventManager.Instance?.Unsubscribe<GameInitializedEvent>(OnGameInitialized);
        }

        /// <summary>
        /// 刷新所有顯示
        /// </summary>
        public void RefreshAll()
        {
            RefreshPlayerInfo();
            RefreshResources();
            RefreshSoldierCount();
        }

        /// <summary>
        /// 刷新玩家資訊
        /// </summary>
        private void RefreshPlayerInfo()
        {
            if (playerNameText != null)
                playerNameText.text = GameManager.Instance?.GetPlayerName() ?? "未知玩家";

            if (playerLevelText != null)
                playerLevelText.text = $"Lv{GameManager.Instance?.GetPlayerLevel() ?? 1}";
        }

        /// <summary>
        /// 刷新資源顯示
        /// </summary>
        private void RefreshResources()
        {
            var resources = ResourceManager.Instance?.PlayerResources;
            if (resources == null) return;

            if (copperText != null)
                copperText.text = FormatNumber(resources.Copper);

            if (woodText != null)
                woodText.text = FormatNumber(resources.Wood);

            if (stoneText != null)
                stoneText.text = FormatNumber(resources.Stone);

            if (foodText != null)
                foodText.text = FormatNumber(resources.Food);
        }

        /// <summary>
        /// 刷新士兵數量
        /// </summary>
        private void RefreshSoldierCount()
        {
            var army = ArmyManager.Instance?.PlayerArmy;
            if (army == null) return;

            if (soldierCountText != null)
                soldierCountText.text = $"{army.TotalSoldiers}/{Game.Data.PlayerArmy.MaxSoldiers}";
        }

        /// <summary>
        /// 格式化數字顯示
        /// </summary>
        private string FormatNumber(int value)
        {
            if (value >= 1000000)
                return $"{value / 1000000f:F1}M";
            if (value >= 1000)
                return $"{value / 1000f:F1}K";
            return value.ToString();
        }

        #region 按鈕點擊事件

        private void OnTerritoryButtonClick()
        {
            UIManager.Instance?.OpenPanel<TerritoryPanel>();
        }

        private void OnArmyButtonClick()
        {
            UIManager.Instance?.OpenPanel<ArmyPanel>();
        }

        private void OnGeneralButtonClick()
        {
            UIManager.Instance?.OpenPanel<GeneralPanel>();
        }

        private void OnMapButtonClick()
        {
            Debug.Log("[GameHUD] 地圖功能開發中...");
            // TODO: UIManager.Instance?.OpenPanel<MapPanel>();
        }

        /// <summary>
        /// 測試戰鬥按鈕（開發用）
        /// </summary>
        public void OnTestBattleButtonClick()
        {
            var tester = FindObjectOfType<BattleTester>();
            if (tester == null)
            {
                var testerObj = new GameObject("BattleTester");
                tester = testerObj.AddComponent<BattleTester>();
            }

            tester.CreateTestBattle();
        }

        private void OnQuestButtonClick()
        {
            Debug.Log("[GameHUD] 任務功能開發中...");
            // TODO: UIManager.Instance?.OpenPanel<QuestPanel>();
        }

        private void OnSettingsButtonClick()
        {
            Debug.Log("[GameHUD] 設定功能開發中...");
            // TODO: UIManager.Instance?.OpenPanel<SettingsPanel>();
        }

        #endregion

        #region 事件處理

        private void OnResourceChanged(ResourceChangedEvent evt)
        {
            RefreshResources();
        }

        private void OnSoldiersChanged(SoldiersChangedEvent evt)
        {
            RefreshSoldierCount();
        }

        private void OnGameInitialized(GameInitializedEvent evt)
        {
            RefreshAll();
        }

        #endregion
    }
}

