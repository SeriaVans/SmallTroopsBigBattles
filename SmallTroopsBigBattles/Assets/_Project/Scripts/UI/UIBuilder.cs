using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// UI 自動建立器 - 自動生成所有 UI 介面
    /// 使用方法: 在場景中建立空物件，加入此腳本，運行遊戲即可
    /// </summary>
    public class UIBuilder : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private bool _buildOnStart = true;
        [SerializeField] private bool _destroyAfterBuild = true;

        private Canvas _mainCanvas;
        private Transform _normalLayer;
        private Transform _popupLayer;
        private Transform _hudLayer;

        // 顏色設定
        private Color _panelBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        private Color _headerBgColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        private Color _buttonColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        private Color _buttonTextColor = Color.white;
        private Color _textColor = Color.white;

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildAllUI();
                if (_destroyAfterBuild)
                {
                    Destroy(gameObject);
                }
            }
        }

        [ContextMenu("Build All UI")]
        public void BuildAllUI()
        {
            Debug.Log("[UIBuilder] 開始建立 UI...");

            // 1. 建立主 Canvas
            CreateMainCanvas();

            // 2. 建立 UIManager
            CreateUIManager();

            // 3. 建立 HUD
            CreateHUD();

            // 4. 建立所有面板預製體
            CreateAllPanelPrefabs();

            Debug.Log("[UIBuilder] UI 建立完成！");
        }

        #region Canvas 與 UIManager

        private void CreateMainCanvas()
        {
            // 主 Canvas
            var canvasObj = new GameObject("MainCanvas");
            _mainCanvas = canvasObj.AddComponent<Canvas>();
            _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _mainCanvas.sortingOrder = 0;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 建立層級
            _hudLayer = CreateUILayer(canvasObj.transform, "HUDLayer", 0);
            _normalLayer = CreateUILayer(canvasObj.transform, "NormalLayer", 10);
            _popupLayer = CreateUILayer(canvasObj.transform, "PopupLayer", 20);
        }

        private Transform CreateUILayer(Transform parent, string name, int sortOrder)
        {
            var layerObj = new GameObject(name);
            layerObj.transform.SetParent(parent, false);

            var rect = layerObj.AddComponent<RectTransform>();
            SetFullStretch(rect);

            return layerObj.transform;
        }

        private void CreateUIManager()
        {
            var existing = FindFirstObjectByType<UIManager>();
            if (existing != null) return;

            var managerObj = new GameObject("[UIManager]");
            var uiManager = managerObj.AddComponent<UIManager>();
            DontDestroyOnLoad(managerObj);
        }

        #endregion

        #region HUD

        private void CreateHUD()
        {
            var hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(_hudLayer, false);
            var hudRect = hudObj.AddComponent<RectTransform>();
            SetFullStretch(hudRect);

            var hud = hudObj.AddComponent<HUD.GameHUD>();

            // 頂部資源列
            var topBar = CreatePanel(hudObj.transform, "TopResourceBar", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 40), _headerBgColor);

            float xPos = 50;
            var copperText = CreateText(topBar, "CopperText", $"銅: 0", new Vector2(xPos, 0)); xPos += 200;
            var woodText = CreateText(topBar, "WoodText", $"木: 0", new Vector2(xPos, 0)); xPos += 200;
            var stoneText = CreateText(topBar, "StoneText", $"石: 0", new Vector2(xPos, 0)); xPos += 200;
            var foodText = CreateText(topBar, "FoodText", $"糧: 0", new Vector2(xPos, 0));

            // 玩家資訊 (右上)
            var playerName = CreateText(topBar, "PlayerNameText", "玩家", new Vector2(-200, 0));
            playerName.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            playerName.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);

            var playerLevel = CreateText(topBar, "PlayerLevelText", "Lv.1", new Vector2(-80, 0));
            playerLevel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            playerLevel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);

            // 底部功能列
            var bottomBar = CreatePanel(hudObj.transform, "BottomBar", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(0, -50), _headerBgColor);

            var territoryBtn = CreateButton(bottomBar, "TerritoryButton", "領地", new Vector2(-400, 0), new Vector2(120, 60));
            var armyBtn = CreateButton(bottomBar, "ArmyButton", "軍隊", new Vector2(-240, 0), new Vector2(120, 60));
            var generalBtn = CreateButton(bottomBar, "GeneralButton", "將領", new Vector2(-80, 0), new Vector2(120, 60));
            var mapBtn = CreateButton(bottomBar, "MapButton", "地圖", new Vector2(80, 0), new Vector2(120, 60));
            var settingsBtn = CreateButton(bottomBar, "SettingsButton", "設定", new Vector2(240, 0), new Vector2(120, 60));

            // 軍隊顯示
            var soldierText = CreateText(topBar, "SoldierCountText", "兵: 0/5000", new Vector2(-400, 0));
            soldierText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            soldierText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);

            // 連接 HUD 腳本
            ConnectHUD(hud, copperText, woodText, stoneText, foodText, soldierText,
                playerName, playerLevel, territoryBtn, armyBtn, generalBtn, mapBtn, settingsBtn);
        }

        private void ConnectHUD(HUD.GameHUD hud, GameObject copper, GameObject wood, GameObject stone, GameObject food,
            GameObject soldier, GameObject playerName, GameObject playerLevel,
            GameObject territory, GameObject army, GameObject general, GameObject map, GameObject settings)
        {
            var type = typeof(HUD.GameHUD);
            SetPrivateField(hud, "_copperText", copper.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_woodText", wood.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_stoneText", stone.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_foodText", food.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_soldierCountText", soldier.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_playerNameText", playerName.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_playerLevelText", playerLevel.GetComponent<TextMeshProUGUI>());
            SetPrivateField(hud, "_territoryButton", territory.GetComponent<Button>());
            SetPrivateField(hud, "_armyButton", army.GetComponent<Button>());
            SetPrivateField(hud, "_generalButton", general.GetComponent<Button>());
            SetPrivateField(hud, "_mapButton", map.GetComponent<Button>());
            SetPrivateField(hud, "_settingsButton", settings.GetComponent<Button>());
        }

        #endregion

        #region Panel Prefabs

        private void CreateAllPanelPrefabs()
        {
            var territoryPanel = CreateTerritoryPanel();
            var armyPanel = CreateArmyPanel();
            var generalPanel = CreateGeneralPanel();
            var mapPanel = CreateMapPanel();
            var settingsPanel = CreateSettingsPanel();

            // 註冊到 UIManager
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.RegisterPanelPrefab(territoryPanel);
                uiManager.RegisterPanelPrefab(armyPanel);
                uiManager.RegisterPanelPrefab(generalPanel);
                uiManager.RegisterPanelPrefab(mapPanel);
                uiManager.RegisterPanelPrefab(settingsPanel);
            }
        }

        private TerritoryPanel CreateTerritoryPanel()
        {
            var panelObj = CreateBasePanelObject("TerritoryPanel", "領地管理");
            var panel = panelObj.AddComponent<TerritoryPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            // 領地標籤容器
            var tabContainer = CreateEmptyRect(content, "TerritoryTabs", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -100), new Vector2(0, 50));
            var hlg = tabContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;

            // 領地標籤預製體
            var tabPrefab = CreateButton(null, "TerritoryTab", "領地 1", Vector2.zero, new Vector2(120, 40));
            tabPrefab.SetActive(false);
            tabPrefab.transform.SetParent(panelObj.transform, false);

            // 資訊顯示
            var infoPanel = CreatePanel(content, "InfoPanel", new Vector2(0, 1), new Vector2(0.3f, 1),
                new Vector2(150, -180), new Vector2(0, -160), new Color(0, 0, 0, 0.3f));
            var territoryNameText = CreateText(infoPanel.transform, "TerritoryNameText", "領地 1", new Vector2(0, 60));
            var buildingCountText = CreateText(infoPanel.transform, "BuildingCountText", "建築: 0/15", new Vector2(0, 20));

            // 建築格容器
            var slotContainer = CreateEmptyRect(content, "BuildingSlotContainer", new Vector2(0.3f, 0), new Vector2(1, 1),
                new Vector2(20, 20), new Vector2(-20, -160));
            var glg = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(100, 100);
            glg.spacing = new Vector2(10, 10);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;

            // 建築格預製體
            var slotPrefab = CreateBuildingSlotPrefab();
            slotPrefab.transform.SetParent(panelObj.transform, false);
            slotPrefab.SetActive(false);

            // 建築詳情彈窗
            var detailPopup = CreatePanel(panelObj.transform, "BuildingDetailPopup", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(350, 280), _panelBgColor);
            detailPopup.SetActive(false);

            var buildingNameText = CreateText(detailPopup.transform, "BuildingNameText", "建築名稱", new Vector2(0, 80));
            var buildingLevelText = CreateText(detailPopup.transform, "BuildingLevelText", "Lv.1", new Vector2(0, 40));
            var buildButton = CreateButton(detailPopup.transform, "BuildButton", "建造", new Vector2(-80, -80), new Vector2(120, 50));
            var upgradeButton = CreateButton(detailPopup.transform, "UpgradeButton", "升級", new Vector2(80, -80), new Vector2(120, 50));

            // 連接
            SetPrivateField(panel, "_closeButton", panelObj.transform.Find("Header/CloseButton").GetComponent<Button>());
            SetPrivateField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            SetPrivateField(panel, "_territoryTabContainer", tabContainer);
            SetPrivateField(panel, "_territoryTabPrefab", tabPrefab.GetComponent<Button>());
            SetPrivateField(panel, "_buildingSlotContainer", slotContainer);
            SetPrivateField(panel, "_buildingSlotPrefab", slotPrefab.GetComponent<BuildingSlotUI>());
            SetPrivateField(panel, "_territoryNameText", territoryNameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_buildingCountText", buildingCountText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_buildingDetailPopup", detailPopup);
            SetPrivateField(panel, "_buildingNameText", buildingNameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_buildingLevelText", buildingLevelText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_buildButton", buildButton.GetComponent<Button>());
            SetPrivateField(panel, "_upgradeButton", upgradeButton.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            return panel;
        }

        private GameObject CreateBuildingSlotPrefab()
        {
            var slotObj = new GameObject("BuildingSlot");
            var rect = slotObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);

            var img = slotObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            var btn = slotObj.AddComponent<Button>();
            btn.targetGraphic = img;

            var slotUI = slotObj.AddComponent<BuildingSlotUI>();

            // 等級文字
            var levelText = CreateText(slotObj.transform, "LevelText", "", Vector2.zero);
            levelText.GetComponent<TextMeshProUGUI>().fontSize = 18;

            // 空格指示
            var emptyIndicator = CreateText(slotObj.transform, "EmptyIndicator", "+", Vector2.zero);
            emptyIndicator.GetComponent<TextMeshProUGUI>().fontSize = 36;

            // 建造中指示
            var constructing = new GameObject("ConstructingIndicator");
            constructing.transform.SetParent(slotObj.transform, false);
            var constructingText = CreateText(constructing.transform, "TimeText", "00:00", Vector2.zero);
            constructingText.GetComponent<TextMeshProUGUI>().fontSize = 16;

            SetPrivateField(slotUI, "_button", btn);
            SetPrivateField(slotUI, "_levelText", levelText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(slotUI, "_emptyIndicator", emptyIndicator);
            SetPrivateField(slotUI, "_constructingIndicator", constructing);
            SetPrivateField(slotUI, "_constructingTimeText", constructingText.GetComponent<TextMeshProUGUI>());

            return slotObj;
        }

        private ArmyPanel CreateArmyPanel()
        {
            var panelObj = CreateBasePanelObject("ArmyPanel", "軍隊管理");
            var panel = panelObj.AddComponent<ArmyPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            // 左側 - 士兵數量
            var leftPanel = CreatePanel(content, "SoldierCountPanel", new Vector2(0, 0), new Vector2(0.4f, 1),
                new Vector2(20, 20), new Vector2(0, -80), new Color(0, 0, 0, 0.3f));

            var spearmanText = CreateText(leftPanel.transform, "SpearmanCountText", "槍兵: 0", new Vector2(0, 100));
            var shieldmanText = CreateText(leftPanel.transform, "ShieldmanCountText", "盾兵: 0", new Vector2(0, 60));
            var cavalryText = CreateText(leftPanel.transform, "CavalryCountText", "騎兵: 0", new Vector2(0, 20));
            var archerText = CreateText(leftPanel.transform, "ArcherCountText", "弓兵: 0", new Vector2(0, -20));
            var totalText = CreateText(leftPanel.transform, "TotalCountText", "總計: 0/5000", new Vector2(0, -80));

            // 右側 - 訓練控制
            var rightPanel = CreatePanel(content, "TrainingPanel", new Vector2(0.4f, 0), new Vector2(1, 1),
                new Vector2(0, 20), new Vector2(-20, -80), new Color(0, 0, 0, 0.3f));

            var typeLabel = CreateText(rightPanel.transform, "TypeLabel", "兵種:", new Vector2(0, 100));
            var dropdown = CreateDropdown(rightPanel.transform, "SoldierTypeDropdown", new Vector2(0, 60), new Vector2(200, 40));

            var countLabel = CreateText(rightPanel.transform, "CountLabel", "數量:", new Vector2(0, 10));
            var slider = CreateSlider(rightPanel.transform, "TrainCountSlider", new Vector2(0, -30), new Vector2(200, 30));
            var input = CreateInputField(rightPanel.transform, "TrainCountInput", "10", new Vector2(0, -70), new Vector2(100, 40));

            var costText = CreateText(rightPanel.transform, "TrainCostText", "費用: -", new Vector2(0, -110));
            var trainBtn = CreateButton(rightPanel.transform, "TrainButton", "訓練", new Vector2(0, -160), new Vector2(150, 50));

            // 底部 - 訓練佇列
            var queuePanel = CreatePanel(content, "TrainingQueuePanel", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(20, 20), new Vector2(-20, 80), new Color(0, 0, 0, 0.3f));

            var progressBg = CreateImage(queuePanel.transform, "ProgressBg", new Vector2(0, 0), new Vector2(300, 30), new Color(0.2f, 0.2f, 0.2f, 1));
            var progressBar = CreateImage(queuePanel.transform, "TrainingProgressBar", new Vector2(0, 0), new Vector2(300, 30), new Color(0.3f, 0.7f, 0.3f, 1));
            progressBar.GetComponent<Image>().type = Image.Type.Filled;
            progressBar.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            progressBar.GetComponent<Image>().fillAmount = 0.5f;

            var progressText = CreateText(queuePanel.transform, "TrainingProgressText", "訓練中...", new Vector2(180, 0));

            // 連接
            SetPrivateField(panel, "_closeButton", panelObj.transform.Find("Header/CloseButton").GetComponent<Button>());
            SetPrivateField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            SetPrivateField(panel, "_spearmanCountText", spearmanText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_shieldmanCountText", shieldmanText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_cavalryCountText", cavalryText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_archerCountText", archerText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_totalCountText", totalText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_soldierTypeDropdown", dropdown.GetComponent<TMP_Dropdown>());
            SetPrivateField(panel, "_trainCountSlider", slider.GetComponent<Slider>());
            SetPrivateField(panel, "_trainCountInput", input.GetComponent<TMP_InputField>());
            SetPrivateField(panel, "_trainCostText", costText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_trainButton", trainBtn.GetComponent<Button>());
            SetPrivateField(panel, "_trainingQueuePanel", queuePanel);
            SetPrivateField(panel, "_trainingProgressBar", progressBar.GetComponent<Image>());
            SetPrivateField(panel, "_trainingProgressText", progressText.GetComponent<TextMeshProUGUI>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            return panel;
        }

        private GeneralPanel CreateGeneralPanel()
        {
            var panelObj = CreateBasePanelObject("GeneralPanel", "將領");
            var panel = panelObj.AddComponent<GeneralPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");
            var header = panelObj.transform.Find("Header");

            var countText = CreateText(header, "GeneralCountText", "將領: 0", new Vector2(150, 0));

            // 左側 - 將領列表
            var listPanel = CreatePanel(content, "ListPanel", new Vector2(0, 0), new Vector2(0.35f, 1),
                new Vector2(10, 60), new Vector2(0, -10), new Color(0, 0, 0, 0.3f));

            var listContainer = CreateEmptyRect(listPanel.transform, "GeneralListContainer", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, 0));
            SetFullStretch(listContainer);
            var vlg = listContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;

            // 將領項目預製體
            var itemPrefab = CreateGeneralListItemPrefab();
            itemPrefab.transform.SetParent(panelObj.transform, false);
            itemPrefab.SetActive(false);

            // 右側 - 詳情
            var detailPanel = CreatePanel(content, "DetailPanel", new Vector2(0.35f, 0), new Vector2(1, 1),
                new Vector2(0, 60), new Vector2(-10, -10), new Color(0, 0, 0, 0.3f));
            detailPanel.SetActive(false);

            var nameText = CreateText(detailPanel.transform, "GeneralNameText", "將領名", new Vector2(0, 120));
            nameText.GetComponent<TextMeshProUGUI>().fontSize = 32;
            var classText = CreateText(detailPanel.transform, "GeneralClassText", "職業", new Vector2(0, 80));
            var levelText = CreateText(detailPanel.transform, "GeneralLevelText", "Lv.1", new Vector2(0, 50));
            var strText = CreateText(detailPanel.transform, "StrengthText", "武力: 0", new Vector2(-80, 0));
            var intText = CreateText(detailPanel.transform, "IntelligenceText", "智力: 0", new Vector2(80, 0));
            var cmdText = CreateText(detailPanel.transform, "CommandText", "統帥: 0", new Vector2(-80, -40));
            var spdText = CreateText(detailPanel.transform, "SpeedText", "速度: 0", new Vector2(80, -40));
            var powerText = CreateText(detailPanel.transform, "PowerText", "戰力: 0", new Vector2(0, -90));
            powerText.GetComponent<TextMeshProUGUI>().fontSize = 28;

            var dismissBtn = CreateButton(detailPanel.transform, "DismissButton", "遣散", new Vector2(0, -150), new Vector2(120, 45));

            // 底部招募按鈕
            var recruitBtn = CreateButton(content, "RecruitButton", "招募將領", new Vector2(0, 30), new Vector2(200, 50));
            recruitBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            recruitBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);

            // 連接
            SetPrivateField(panel, "_closeButton", panelObj.transform.Find("Header/CloseButton").GetComponent<Button>());
            SetPrivateField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            SetPrivateField(panel, "_generalListContainer", listContainer);
            SetPrivateField(panel, "_generalItemPrefab", itemPrefab.GetComponent<GeneralListItemUI>());
            SetPrivateField(panel, "_generalCountText", countText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_detailPanel", detailPanel);
            SetPrivateField(panel, "_generalNameText", nameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_generalClassText", classText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_generalLevelText", levelText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_strengthText", strText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_intelligenceText", intText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_commandText", cmdText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_speedText", spdText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_powerText", powerText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(panel, "_recruitButton", recruitBtn.GetComponent<Button>());
            SetPrivateField(panel, "_dismissButton", dismissBtn.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            return panel;
        }

        private GameObject CreateGeneralListItemPrefab()
        {
            var itemObj = new GameObject("GeneralListItem");
            var rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 80);

            var img = itemObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            var btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = img;

            var itemUI = itemObj.AddComponent<GeneralListItemUI>();

            var frame = CreateImage(itemObj.transform, "Frame", Vector2.zero, new Vector2(280, 80), Color.gray);
            frame.GetComponent<Image>().type = Image.Type.Sliced;
            SetFullStretch(frame.GetComponent<RectTransform>());

            var nameText = CreateText(itemObj.transform, "NameText", "將領名", new Vector2(-40, 15));
            nameText.GetComponent<TextMeshProUGUI>().fontSize = 22;
            var levelText = CreateText(itemObj.transform, "LevelText", "Lv.1", new Vector2(-40, -15));
            levelText.GetComponent<TextMeshProUGUI>().fontSize = 18;
            var classText = CreateText(itemObj.transform, "ClassText", "統帥", new Vector2(80, 0));
            classText.GetComponent<TextMeshProUGUI>().fontSize = 18;

            SetPrivateField(itemUI, "_button", btn);
            SetPrivateField(itemUI, "_nameText", nameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(itemUI, "_levelText", levelText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(itemUI, "_classText", classText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(itemUI, "_frameImage", frame.GetComponent<Image>());

            return itemObj;
        }

        private MapPanel CreateMapPanel()
        {
            var panelObj = CreateBasePanelObject("MapPanel", "世界地圖");
            var panel = panelObj.AddComponent<MapPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");
            var mapTitle = CreateText(content, "MapTitleText", "地圖功能開發中...", Vector2.zero);
            mapTitle.GetComponent<TextMeshProUGUI>().fontSize = 36;

            SetPrivateField(panel, "_closeButton", panelObj.transform.Find("Header/CloseButton").GetComponent<Button>());
            SetPrivateField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            SetPrivateField(panel, "_mapTitleText", mapTitle.GetComponent<TextMeshProUGUI>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            return panel;
        }

        private SettingsPanel CreateSettingsPanel()
        {
            var panelObj = CreateBasePanelObject("SettingsPanel", "設定", new Vector2(500, 400));
            var panel = panelObj.AddComponent<SettingsPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            var bgmLabel = CreateText(content, "BGMLabel", "背景音樂", new Vector2(0, 80));
            var bgmSlider = CreateSlider(content, "BGMVolumeSlider", new Vector2(0, 40), new Vector2(300, 30));

            var sfxLabel = CreateText(content, "SFXLabel", "音效", new Vector2(0, -20));
            var sfxSlider = CreateSlider(content, "SFXVolumeSlider", new Vector2(0, -60), new Vector2(300, 30));

            var saveBtn = CreateButton(content, "SaveButton", "儲存", new Vector2(0, -130), new Vector2(150, 50));

            SetPrivateField(panel, "_closeButton", panelObj.transform.Find("Header/CloseButton").GetComponent<Button>());
            SetPrivateField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            SetPrivateField(panel, "_bgmVolumeSlider", bgmSlider.GetComponent<Slider>());
            SetPrivateField(panel, "_sfxVolumeSlider", sfxSlider.GetComponent<Slider>());
            SetPrivateField(panel, "_saveButton", saveBtn.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            return panel;
        }

        #endregion

        #region UI Helpers

        private GameObject CreateBasePanelObject(string name, string title, Vector2? size = null)
        {
            var panelSize = size ?? new Vector2(900, 600);

            var panelObj = new GameObject(name);
            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = panelSize;

            var bg = panelObj.AddComponent<Image>();
            bg.color = _panelBgColor;

            // Header
            var header = CreatePanel(panelObj.transform, "Header", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -35), new Vector2(0, 35), _headerBgColor);

            var titleText = CreateText(header.transform, "TitleText", title, Vector2.zero);
            titleText.GetComponent<TextMeshProUGUI>().fontSize = 28;

            var closeBtn = CreateButton(header.transform, "CloseButton", "X", new Vector2(-30, 0), new Vector2(50, 50));
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);

            // Content Area
            var content = CreateEmptyRect(panelObj.transform, "Content", new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -70));
            SetFullStretch(content);
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, -70);

            return panelObj;
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var img = obj.AddComponent<Image>();
            img.color = color;

            return obj;
        }

        private RectTransform CreateEmptyRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return rect;
        }

        private GameObject CreateText(Transform parent, string name, string text, Vector2 position)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 40);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            if (parent != null) obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = _buttonColor;

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            SetFullStretch(textRect);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.color = _buttonTextColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = color;

            return obj;
        }

        private GameObject CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            // Background
            var bg = CreateImage(obj.transform, "Background", Vector2.zero, size, new Color(0.2f, 0.2f, 0.2f, 1));
            SetFullStretch(bg.GetComponent<RectTransform>());

            // Fill Area
            var fillArea = CreateEmptyRect(obj.transform, "Fill Area", Vector2.zero, Vector2.one, new Vector2(5, 0), new Vector2(-5, 0));
            var fill = CreateImage(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, _buttonColor);
            SetFullStretch(fill.GetComponent<RectTransform>());

            // Handle
            var handleArea = CreateEmptyRect(obj.transform, "Handle Slide Area", Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var handle = CreateImage(handleArea.transform, "Handle", Vector2.zero, new Vector2(20, 0), Color.white);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = new Vector2(0, 1);
            handleRect.sizeDelta = new Vector2(20, 0);

            var slider = obj.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 50;

            return obj;
        }

        private GameObject CreateDropdown(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f, 1);

            // Label
            var label = CreateText(obj.transform, "Label", "選擇...", Vector2.zero);
            SetFullStretch(label.GetComponent<RectTransform>());
            label.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            label.GetComponent<RectTransform>().offsetMax = new Vector2(-30, 0);
            label.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Arrow
            var arrow = CreateText(obj.transform, "Arrow", "▼", new Vector2(-15, 0));
            arrow.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            arrow.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
            arrow.GetComponent<TextMeshProUGUI>().fontSize = 16;

            // Template
            var template = new GameObject("Template");
            template.transform.SetParent(obj.transform, false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0, 150);
            template.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            SetFullStretch(viewportRect);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>();

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 28);

            var item = new GameObject("Item");
            item.transform.SetParent(contentObj.transform, false);
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 28);

            var itemToggle = item.AddComponent<Toggle>();
            var itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.3f, 0.3f, 0.35f, 1);
            itemToggle.targetGraphic = itemBg;

            var itemLabel = CreateText(item.transform, "Item Label", "Option", Vector2.zero);
            SetFullStretch(itemLabel.GetComponent<RectTransform>());
            itemLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            itemLabel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);

            template.SetActive(false);

            var dropdown = obj.AddComponent<TMP_Dropdown>();
            dropdown.template = templateRect;
            dropdown.captionText = label.GetComponent<TextMeshProUGUI>();
            dropdown.itemText = itemLabel.GetComponent<TextMeshProUGUI>();

            return obj;
        }

        private GameObject CreateInputField(Transform parent, string name, string defaultValue, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.2f, 1);

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(obj.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            SetFullStretch(textAreaRect);
            textAreaRect.offsetMin = new Vector2(10, 0);
            textAreaRect.offsetMax = new Vector2(-10, 0);
            textArea.AddComponent<RectMask2D>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            SetFullStretch(textRect);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 20;
            tmp.color = _textColor;

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            var phRect = placeholder.AddComponent<RectTransform>();
            SetFullStretch(phRect);
            var phTmp = placeholder.AddComponent<TextMeshProUGUI>();
            phTmp.text = "輸入...";
            phTmp.fontSize = 20;
            phTmp.color = new Color(1, 1, 1, 0.5f);
            phTmp.fontStyle = FontStyles.Italic;

            var input = obj.AddComponent<TMP_InputField>();
            input.textViewport = textAreaRect;
            input.textComponent = tmp;
            input.placeholder = phTmp;
            input.text = defaultValue;

            return obj;
        }

        private void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[UIBuilder] 找不到欄位: {obj.GetType().Name}.{fieldName}");
            }
        }

        #endregion
    }
}
