#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace SmallTroopsBigBattles.Editor
{
    /// <summary>
    /// 遊戲 UI 建立工具 - 從 Unity 選單執行
    /// 使用方法: Tools → Small Troops Big Battles → 建立遊戲 UI
    /// </summary>
    public class GameUICreator : EditorWindow
    {
        // 顏色設定
        private static Color _panelBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        private static Color _headerBgColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        private static Color _buttonColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        private static Color _buttonTextColor = Color.white;
        private static Color _textColor = Color.white;

        private static Canvas _mainCanvas;
        private static Transform _normalLayer;
        private static Transform _popupLayer;
        private static Transform _hudLayer;

        [MenuItem("Tools/Small Troops Big Battles/建立遊戲 UI", false, 1)]
        public static void CreateGameUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("錯誤", "請先停止遊戲再執行此操作", "確定");
                return;
            }

            if (!EditorUtility.DisplayDialog("建立遊戲 UI",
                "這將建立以下內容:\n\n" +
                "• MainCanvas (主畫布)\n" +
                "• GameHUD (資源列 + 功能按鈕)\n" +
                "• UIManager (UI 管理器)\n" +
                "• 所有 UI 面板\n\n" +
                "確定要建立嗎?", "建立", "取消"))
            {
                return;
            }

            Debug.Log("[GameUICreator] 開始建立 UI...");

            // 1. 建立主 Canvas
            CreateMainCanvas();

            // 2. 建立 Managers
            CreateManagers();

            // 3. 建立 HUD
            CreateHUD();

            // 4. 建立所有面板
            CreateAllPanels();

            Debug.Log("[GameUICreator] UI 建立完成！");
            EditorUtility.DisplayDialog("完成", "遊戲 UI 建立完成！\n\n請點擊 Play 測試遊戲。", "確定");
        }

        [MenuItem("Tools/Small Troops Big Battles/建立 Managers", false, 2)]
        public static void CreateManagersOnly()
        {
            CreateManagers();
            EditorUtility.DisplayDialog("完成", "Managers 建立完成！", "確定");
        }

        [MenuItem("Tools/Small Troops Big Battles/清除所有 UI", false, 100)]
        public static void ClearAllUI()
        {
            if (!EditorUtility.DisplayDialog("清除 UI", "確定要刪除所有 UI 物件嗎?", "刪除", "取消"))
                return;

            // 刪除 Canvas
            var canvas = GameObject.Find("MainCanvas");
            if (canvas != null) DestroyImmediate(canvas);

            // 刪除 Managers
            var managers = new[] { "[GameManager]", "[EventManager]", "[ResourceManager]",
                "[TerritoryManager]", "[ArmyManager]", "[GeneralManager]", "[UIManager]" };
            foreach (var name in managers)
            {
                var obj = GameObject.Find(name);
                if (obj != null) DestroyImmediate(obj);
            }

            Debug.Log("[GameUICreator] 已清除所有 UI");
        }

        #region Canvas & Managers

        private static void CreateMainCanvas()
        {
            // 檢查是否已存在
            var existing = GameObject.Find("MainCanvas");
            if (existing != null)
            {
                _mainCanvas = existing.GetComponent<Canvas>();
                _hudLayer = existing.transform.Find("HUDLayer");
                _normalLayer = existing.transform.Find("NormalLayer");
                _popupLayer = existing.transform.Find("PopupLayer");
                if (_hudLayer != null && _normalLayer != null && _popupLayer != null)
                {
                    Debug.Log("[GameUICreator] 使用現有 Canvas");
                    return;
                }
                DestroyImmediate(existing);
            }

            var canvasObj = new GameObject("MainCanvas");
            _mainCanvas = canvasObj.AddComponent<Canvas>();
            _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            _hudLayer = CreateUILayer(canvasObj.transform, "HUDLayer");
            _normalLayer = CreateUILayer(canvasObj.transform, "NormalLayer");
            _popupLayer = CreateUILayer(canvasObj.transform, "PopupLayer");

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create MainCanvas");
            Debug.Log("[GameUICreator] MainCanvas 建立完成");
        }

        private static Transform CreateUILayer(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            SetFullStretch(rect);
            return obj.transform;
        }

        private static void CreateManagers()
        {
            CreateManager<Core.Managers.GameManager>("[GameManager]");
            CreateManager<Core.Events.EventManager>("[EventManager]");
            CreateManager<Core.Managers.ResourceManager>("[ResourceManager]");
            CreateManager<Core.Managers.TerritoryManager>("[TerritoryManager]");
            CreateManager<Core.Managers.ArmyManager>("[ArmyManager]");
            CreateManager<Core.Managers.GeneralManager>("[GeneralManager]");
            CreateManager<UI.UIManager>("[UIManager]");
            Debug.Log("[GameUICreator] Managers 建立完成");
        }

        private static void CreateManager<T>(string name) where T : Component
        {
            var existing = GameObject.Find(name);
            if (existing != null) return;

            var obj = new GameObject(name);
            obj.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(obj, $"Create {name}");
        }

        #endregion

        #region HUD

        private static void CreateHUD()
        {
            if (_hudLayer == null) return;

            var existing = _hudLayer.Find("GameHUD");
            if (existing != null)
            {
                Debug.Log("[GameUICreator] HUD 已存在");
                return;
            }

            var hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(_hudLayer, false);
            var hudRect = hudObj.AddComponent<RectTransform>();
            SetFullStretch(hudRect);

            var hud = hudObj.AddComponent<UI.HUD.GameHUD>();

            // 頂部資源列
            var topBar = CreatePanel(hudObj.transform, "TopResourceBar",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 40), _headerBgColor);

            float xPos = 50;
            var copperText = CreateText(topBar.transform, "CopperText", "銅: 0", new Vector2(xPos, 0)); xPos += 200;
            var woodText = CreateText(topBar.transform, "WoodText", "木: 0", new Vector2(xPos, 0)); xPos += 200;
            var stoneText = CreateText(topBar.transform, "StoneText", "石: 0", new Vector2(xPos, 0)); xPos += 200;
            var foodText = CreateText(topBar.transform, "FoodText", "糧: 0", new Vector2(xPos, 0));

            var playerName = CreateText(topBar.transform, "PlayerNameText", "玩家", new Vector2(-200, 0));
            SetAnchor(playerName, new Vector2(1, 0.5f), new Vector2(1, 0.5f));

            var playerLevel = CreateText(topBar.transform, "PlayerLevelText", "Lv.1", new Vector2(-80, 0));
            SetAnchor(playerLevel, new Vector2(1, 0.5f), new Vector2(1, 0.5f));

            var soldierText = CreateText(topBar.transform, "SoldierCountText", "兵: 0/5000", new Vector2(-400, 0));
            SetAnchor(soldierText, new Vector2(1, 0.5f), new Vector2(1, 0.5f));

            // 底部功能列
            var bottomBar = CreatePanel(hudObj.transform, "BottomBar",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(0, -50), _headerBgColor);

            var territoryBtn = CreateButton(bottomBar.transform, "TerritoryButton", "領地", new Vector2(-400, 0), new Vector2(120, 60));
            var armyBtn = CreateButton(bottomBar.transform, "ArmyButton", "軍隊", new Vector2(-240, 0), new Vector2(120, 60));
            var generalBtn = CreateButton(bottomBar.transform, "GeneralButton", "將領", new Vector2(-80, 0), new Vector2(120, 60));
            var mapBtn = CreateButton(bottomBar.transform, "MapButton", "地圖", new Vector2(80, 0), new Vector2(120, 60));
            var settingsBtn = CreateButton(bottomBar.transform, "SettingsButton", "設定", new Vector2(240, 0), new Vector2(120, 60));

            // 連接 HUD
            ConnectSerializedField(hud, "_copperText", copperText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_woodText", woodText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_stoneText", stoneText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_foodText", foodText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_soldierCountText", soldierText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_playerNameText", playerName.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_playerLevelText", playerLevel.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(hud, "_territoryButton", territoryBtn.GetComponent<Button>());
            ConnectSerializedField(hud, "_armyButton", armyBtn.GetComponent<Button>());
            ConnectSerializedField(hud, "_generalButton", generalBtn.GetComponent<Button>());
            ConnectSerializedField(hud, "_mapButton", mapBtn.GetComponent<Button>());
            ConnectSerializedField(hud, "_settingsButton", settingsBtn.GetComponent<Button>());

            EditorUtility.SetDirty(hud);
            Undo.RegisterCreatedObjectUndo(hudObj, "Create HUD");
            Debug.Log("[GameUICreator] HUD 建立完成");
        }

        #endregion

        #region Panels

        private static void CreateAllPanels()
        {
            if (_normalLayer == null) return;

            var uiManager = GameObject.Find("[UIManager]")?.GetComponent<UI.UIManager>();

            var territory = CreateTerritoryPanel();
            var army = CreateArmyPanel();
            var general = CreateGeneralPanel();
            var map = CreateMapPanel();
            var settings = CreateSettingsPanel();

            if (uiManager != null)
            {
                uiManager.RegisterPanelPrefab(territory);
                uiManager.RegisterPanelPrefab(army);
                uiManager.RegisterPanelPrefab(general);
                uiManager.RegisterPanelPrefab(map);
                uiManager.RegisterPanelPrefab(settings);
                EditorUtility.SetDirty(uiManager);
            }

            Debug.Log("[GameUICreator] 所有面板建立完成");
        }

        private static UI.TerritoryPanel CreateTerritoryPanel()
        {
            var panelObj = CreateBasePanelObject("TerritoryPanel", "領地管理");
            var panel = panelObj.AddComponent<UI.TerritoryPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            // 領地標籤容器
            var tabContainer = CreateEmptyRect(content, "TerritoryTabs",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(10, -100), new Vector2(-10, -60));
            var hlg = tabContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;

            // 標籤預製體
            var tabPrefab = CreateButton(panelObj.transform, "TabPrefab", "領地", Vector2.zero, new Vector2(100, 35));
            tabPrefab.SetActive(false);

            // 資訊
            var nameText = CreateText(content, "TerritoryNameText", "領地 1", new Vector2(-300, 200));
            var countText = CreateText(content, "BuildingCountText", "建築: 0/15", new Vector2(-300, 160));

            // 建築格容器
            var slotContainer = CreateEmptyRect(content, "BuildingSlotContainer",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(10, 10), new Vector2(-10, -110));
            var glg = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(100, 100);
            glg.spacing = new Vector2(10, 10);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;

            // 建築格預製體
            var slotPrefab = CreateBuildingSlotPrefab(panelObj.transform);

            // 詳情彈窗
            var popup = CreatePanel(panelObj.transform, "BuildingDetailPopup",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-175, -120), new Vector2(175, 120), _panelBgColor);
            popup.SetActive(false);

            var buildingName = CreateText(popup.transform, "BuildingNameText", "建築", new Vector2(0, 60));
            var buildingLevel = CreateText(popup.transform, "BuildingLevelText", "Lv.1", new Vector2(0, 20));
            var buildBtn = CreateButton(popup.transform, "BuildButton", "建造", new Vector2(-70, -60), new Vector2(100, 40));
            var upgradeBtn = CreateButton(popup.transform, "UpgradeButton", "升級", new Vector2(70, -60), new Vector2(100, 40));

            // 連接
            var closeBtn = panelObj.transform.Find("Header/CloseButton");
            ConnectSerializedField(panel, "_closeButton", closeBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            ConnectSerializedField(panel, "_territoryTabContainer", tabContainer);
            ConnectSerializedField(panel, "_territoryTabPrefab", tabPrefab.GetComponent<Button>());
            ConnectSerializedField(panel, "_territoryNameText", nameText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_buildingCountText", countText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_buildingSlotContainer", slotContainer);
            ConnectSerializedField(panel, "_buildingSlotPrefab", slotPrefab.GetComponent<UI.BuildingSlotUI>());
            ConnectSerializedField(panel, "_buildingDetailPopup", popup);
            ConnectSerializedField(panel, "_buildingNameText", buildingName.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_buildingLevelText", buildingLevel.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_buildButton", buildBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_upgradeButton", upgradeBtn.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static GameObject CreateBuildingSlotPrefab(Transform parent)
        {
            var obj = new GameObject("SlotPrefab");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var slotUI = obj.AddComponent<UI.BuildingSlotUI>();

            var levelText = CreateText(obj.transform, "LevelText", "", Vector2.zero);
            levelText.GetComponent<TextMeshProUGUI>().fontSize = 18;

            var empty = CreateText(obj.transform, "EmptyIndicator", "+", Vector2.zero);
            empty.GetComponent<TextMeshProUGUI>().fontSize = 36;

            var constructing = new GameObject("ConstructingIndicator");
            constructing.transform.SetParent(obj.transform, false);
            var timeText = CreateText(constructing.transform, "TimeText", "00:00", Vector2.zero);
            timeText.GetComponent<TextMeshProUGUI>().fontSize = 14;

            ConnectSerializedField(slotUI, "_button", btn);
            ConnectSerializedField(slotUI, "_levelText", levelText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(slotUI, "_emptyIndicator", empty);
            ConnectSerializedField(slotUI, "_constructingIndicator", constructing);
            ConnectSerializedField(slotUI, "_constructingTimeText", timeText.GetComponent<TextMeshProUGUI>());

            obj.SetActive(false);
            return obj;
        }

        private static UI.ArmyPanel CreateArmyPanel()
        {
            var panelObj = CreateBasePanelObject("ArmyPanel", "軍隊管理");
            var panel = panelObj.AddComponent<UI.ArmyPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            // 左側士兵數量
            var leftPanel = CreatePanel(content, "SoldierPanel",
                new Vector2(0, 0), new Vector2(0.4f, 1),
                new Vector2(10, 10), new Vector2(0, -10), new Color(0, 0, 0, 0.3f));

            var spearman = CreateText(leftPanel.transform, "SpearmanCountText", "槍兵: 0", new Vector2(0, 80));
            var shieldman = CreateText(leftPanel.transform, "ShieldmanCountText", "盾兵: 0", new Vector2(0, 40));
            var cavalry = CreateText(leftPanel.transform, "CavalryCountText", "騎兵: 0", new Vector2(0, 0));
            var archer = CreateText(leftPanel.transform, "ArcherCountText", "弓兵: 0", new Vector2(0, -40));
            var total = CreateText(leftPanel.transform, "TotalCountText", "總計: 0/5000", new Vector2(0, -100));

            // 右側訓練
            var rightPanel = CreatePanel(content, "TrainingPanel",
                new Vector2(0.4f, 0.3f), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(-10, -10), new Color(0, 0, 0, 0.3f));

            var dropdown = CreateDropdown(rightPanel.transform, "SoldierTypeDropdown", new Vector2(0, 80), new Vector2(200, 40));
            var slider = CreateSlider(rightPanel.transform, "TrainCountSlider", new Vector2(0, 20), new Vector2(200, 30));
            var input = CreateInputField(rightPanel.transform, "TrainCountInput", "10", new Vector2(0, -30), new Vector2(100, 35));
            var costText = CreateText(rightPanel.transform, "TrainCostText", "費用: -", new Vector2(0, -70));
            var trainBtn = CreateButton(rightPanel.transform, "TrainButton", "訓練", new Vector2(0, -120), new Vector2(140, 45));

            // 底部訓練佇列
            var queuePanel = CreatePanel(content, "TrainingQueuePanel",
                new Vector2(0.4f, 0), new Vector2(1, 0.3f),
                new Vector2(0, 10), new Vector2(-10, 0), new Color(0, 0, 0, 0.3f));

            var progressBar = CreateImage(queuePanel.transform, "TrainingProgressBar", Vector2.zero, new Vector2(250, 25), new Color(0.3f, 0.7f, 0.3f, 1));
            progressBar.GetComponent<Image>().type = Image.Type.Filled;
            progressBar.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            var progressText = CreateText(queuePanel.transform, "TrainingProgressText", "", new Vector2(150, 0));

            // 連接
            var closeBtn = panelObj.transform.Find("Header/CloseButton");
            ConnectSerializedField(panel, "_closeButton", closeBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            ConnectSerializedField(panel, "_spearmanCountText", spearman.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_shieldmanCountText", shieldman.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_cavalryCountText", cavalry.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_archerCountText", archer.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_totalCountText", total.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_soldierTypeDropdown", dropdown.GetComponent<TMP_Dropdown>());
            ConnectSerializedField(panel, "_trainCountSlider", slider.GetComponent<Slider>());
            ConnectSerializedField(panel, "_trainCountInput", input.GetComponent<TMP_InputField>());
            ConnectSerializedField(panel, "_trainCostText", costText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_trainButton", trainBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_trainingQueuePanel", queuePanel);
            ConnectSerializedField(panel, "_trainingProgressBar", progressBar.GetComponent<Image>());
            ConnectSerializedField(panel, "_trainingProgressText", progressText.GetComponent<TextMeshProUGUI>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static UI.GeneralPanel CreateGeneralPanel()
        {
            var panelObj = CreateBasePanelObject("GeneralPanel", "將領");
            var panel = panelObj.AddComponent<UI.GeneralPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");
            var header = panelObj.transform.Find("Header");

            var countText = CreateText(header, "GeneralCountText", "將領: 0", new Vector2(120, 0));

            // 左側列表
            var listPanel = CreatePanel(content, "ListPanel",
                new Vector2(0, 0), new Vector2(0.35f, 1),
                new Vector2(10, 50), new Vector2(0, -10), new Color(0, 0, 0, 0.3f));

            var listContainer = CreateEmptyRect(listPanel.transform, "GeneralListContainer",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetFullStretch(listContainer);
            var vlg = listContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;

            // 項目預製體
            var itemPrefab = CreateGeneralItemPrefab(panelObj.transform);

            // 右側詳情
            var detailPanel = CreatePanel(content, "DetailPanel",
                new Vector2(0.35f, 0), new Vector2(1, 1),
                new Vector2(0, 50), new Vector2(-10, -10), new Color(0, 0, 0, 0.3f));
            detailPanel.SetActive(false);

            var nameText = CreateText(detailPanel.transform, "GeneralNameText", "名字", new Vector2(0, 100));
            nameText.GetComponent<TextMeshProUGUI>().fontSize = 30;
            var classText = CreateText(detailPanel.transform, "GeneralClassText", "職業", new Vector2(0, 60));
            var levelText = CreateText(detailPanel.transform, "GeneralLevelText", "Lv.1", new Vector2(0, 30));
            var strText = CreateText(detailPanel.transform, "StrengthText", "武力: 0", new Vector2(-70, -20));
            var intText = CreateText(detailPanel.transform, "IntelligenceText", "智力: 0", new Vector2(70, -20));
            var cmdText = CreateText(detailPanel.transform, "CommandText", "統帥: 0", new Vector2(-70, -55));
            var spdText = CreateText(detailPanel.transform, "SpeedText", "速度: 0", new Vector2(70, -55));
            var powerText = CreateText(detailPanel.transform, "PowerText", "戰力: 0", new Vector2(0, -100));
            var dismissBtn = CreateButton(detailPanel.transform, "DismissButton", "遣散", new Vector2(0, -150), new Vector2(100, 40));

            // 招募按鈕
            var recruitBtn = CreateButton(content, "RecruitButton", "招募將領", new Vector2(0, 20), new Vector2(160, 45));
            SetAnchor(recruitBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0));

            // 連接
            var closeBtn = panelObj.transform.Find("Header/CloseButton");
            ConnectSerializedField(panel, "_closeButton", closeBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            ConnectSerializedField(panel, "_generalListContainer", listContainer);
            ConnectSerializedField(panel, "_generalItemPrefab", itemPrefab.GetComponent<UI.GeneralListItemUI>());
            ConnectSerializedField(panel, "_generalCountText", countText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_detailPanel", detailPanel);
            ConnectSerializedField(panel, "_generalNameText", nameText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_generalClassText", classText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_generalLevelText", levelText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_strengthText", strText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_intelligenceText", intText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_commandText", cmdText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_speedText", spdText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_powerText", powerText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(panel, "_recruitButton", recruitBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_dismissButton", dismissBtn.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static GameObject CreateGeneralItemPrefab(Transform parent)
        {
            var obj = new GameObject("ItemPrefab");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260, 70);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f, 1);

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var itemUI = obj.AddComponent<UI.GeneralListItemUI>();

            var frame = CreateImage(obj.transform, "Frame", Vector2.zero, new Vector2(260, 70), Color.gray);
            SetFullStretch(frame.GetComponent<RectTransform>());

            var nameText = CreateText(obj.transform, "NameText", "名字", new Vector2(-30, 12));
            nameText.GetComponent<TextMeshProUGUI>().fontSize = 20;
            var levelText = CreateText(obj.transform, "LevelText", "Lv.1", new Vector2(-30, -15));
            levelText.GetComponent<TextMeshProUGUI>().fontSize = 16;
            var classText = CreateText(obj.transform, "ClassText", "職業", new Vector2(80, 0));
            classText.GetComponent<TextMeshProUGUI>().fontSize = 16;

            ConnectSerializedField(itemUI, "_button", btn);
            ConnectSerializedField(itemUI, "_nameText", nameText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(itemUI, "_levelText", levelText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(itemUI, "_classText", classText.GetComponent<TextMeshProUGUI>());
            ConnectSerializedField(itemUI, "_frameImage", frame.GetComponent<Image>());

            obj.SetActive(false);
            return obj;
        }

        private static UI.MapPanel CreateMapPanel()
        {
            var panelObj = CreateBasePanelObject("MapPanel", "世界地圖");
            var panel = panelObj.AddComponent<UI.MapPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");
            var titleText = CreateText(content, "MapTitleText", "地圖功能開發中...", Vector2.zero);
            titleText.GetComponent<TextMeshProUGUI>().fontSize = 32;

            var closeBtn = panelObj.transform.Find("Header/CloseButton");
            ConnectSerializedField(panel, "_closeButton", closeBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            ConnectSerializedField(panel, "_mapTitleText", titleText.GetComponent<TextMeshProUGUI>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static UI.SettingsPanel CreateSettingsPanel()
        {
            var panelObj = CreateBasePanelObject("SettingsPanel", "設定", new Vector2(500, 350));
            var panel = panelObj.AddComponent<UI.SettingsPanel>();
            panelObj.AddComponent<CanvasGroup>();

            var content = panelObj.transform.Find("Content");

            CreateText(content, "BGMLabel", "背景音樂", new Vector2(0, 60));
            var bgmSlider = CreateSlider(content, "BGMVolumeSlider", new Vector2(0, 20), new Vector2(280, 25));

            CreateText(content, "SFXLabel", "音效", new Vector2(0, -30));
            var sfxSlider = CreateSlider(content, "SFXVolumeSlider", new Vector2(0, -70), new Vector2(280, 25));

            var saveBtn = CreateButton(content, "SaveButton", "儲存", new Vector2(0, -130), new Vector2(140, 45));

            var closeBtn = panelObj.transform.Find("Header/CloseButton");
            ConnectSerializedField(panel, "_closeButton", closeBtn.GetComponent<Button>());
            ConnectSerializedField(panel, "_canvasGroup", panelObj.GetComponent<CanvasGroup>());
            ConnectSerializedField(panel, "_bgmVolumeSlider", bgmSlider.GetComponent<Slider>());
            ConnectSerializedField(panel, "_sfxVolumeSlider", sfxSlider.GetComponent<Slider>());
            ConnectSerializedField(panel, "_saveButton", saveBtn.GetComponent<Button>());

            panelObj.SetActive(false);
            panelObj.transform.SetParent(_normalLayer, false);
            EditorUtility.SetDirty(panel);
            return panel;
        }

        #endregion

        #region UI Helpers

        private static GameObject CreateBasePanelObject(string name, string title, Vector2? size = null)
        {
            var panelSize = size ?? new Vector2(900, 550);

            var obj = new GameObject(name);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = panelSize;

            var bg = obj.AddComponent<Image>();
            bg.color = _panelBgColor;

            // Header
            var header = CreatePanel(obj.transform, "Header",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -35), new Vector2(0, 35), _headerBgColor);

            var titleText = CreateText(header.transform, "TitleText", title, Vector2.zero);
            titleText.GetComponent<TextMeshProUGUI>().fontSize = 26;

            var closeBtn = CreateButton(header.transform, "CloseButton", "X", new Vector2(-25, 0), new Vector2(45, 45));
            SetAnchor(closeBtn, new Vector2(1, 0.5f), new Vector2(1, 0.5f));

            // Content
            var content = CreateEmptyRect(obj.transform, "Content",
                Vector2.zero, Vector2.one,
                new Vector2(0, 0), new Vector2(0, -70));
            SetFullStretch(content);
            content.offsetMax = new Vector2(0, -70);

            return obj;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
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

        private static RectTransform CreateEmptyRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
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

        private static GameObject CreateText(Transform parent, string name, string text, Vector2 position)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(280, 35);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
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
            tmp.fontSize = 20;
            tmp.color = _buttonTextColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private static GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
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

        private static GameObject CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var bg = CreateImage(obj.transform, "Background", Vector2.zero, size, new Color(0.2f, 0.2f, 0.2f, 1));
            SetFullStretch(bg.GetComponent<RectTransform>());

            var fillArea = CreateEmptyRect(obj.transform, "Fill Area", Vector2.zero, Vector2.one, new Vector2(5, 0), new Vector2(-5, 0));
            var fill = CreateImage(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, _buttonColor);
            SetFullStretch(fill.GetComponent<RectTransform>());

            var handleArea = CreateEmptyRect(obj.transform, "Handle Slide Area", Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var handle = CreateImage(handleArea.transform, "Handle", Vector2.zero, new Vector2(20, 0), Color.white);
            var hRect = handle.GetComponent<RectTransform>();
            hRect.anchorMin = Vector2.zero;
            hRect.anchorMax = new Vector2(0, 1);
            hRect.sizeDelta = new Vector2(20, 0);

            var slider = obj.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = hRect;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 50;

            return obj;
        }

        private static GameObject CreateDropdown(Transform parent, string name, Vector2 position, Vector2 size)
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

            var label = CreateText(obj.transform, "Label", "選擇", Vector2.zero);
            SetFullStretch(label.GetComponent<RectTransform>());
            label.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            label.GetComponent<RectTransform>().offsetMax = new Vector2(-25, 0);
            label.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            var arrow = CreateText(obj.transform, "Arrow", "▼", new Vector2(-12, 0));
            SetAnchor(arrow, new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            arrow.GetComponent<TextMeshProUGUI>().fontSize = 14;

            // Template
            var template = new GameObject("Template");
            template.transform.SetParent(obj.transform, false);
            var tRect = template.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 0);
            tRect.anchorMax = new Vector2(1, 0);
            tRect.pivot = new Vector2(0.5f, 1);
            tRect.sizeDelta = new Vector2(0, 120);
            template.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            SetFullStretch(viewport.AddComponent<RectTransform>());
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>();

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cRect = content.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1);
            cRect.anchorMax = new Vector2(1, 1);
            cRect.pivot = new Vector2(0.5f, 1);
            cRect.sizeDelta = new Vector2(0, 28);

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var iRect = item.AddComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0, 0.5f);
            iRect.anchorMax = new Vector2(1, 0.5f);
            iRect.sizeDelta = new Vector2(0, 28);
            var toggle = item.AddComponent<Toggle>();
            var iBg = item.AddComponent<Image>();
            iBg.color = new Color(0.3f, 0.3f, 0.35f, 1);
            toggle.targetGraphic = iBg;

            var itemLabel = CreateText(item.transform, "Item Label", "Option", Vector2.zero);
            SetFullStretch(itemLabel.GetComponent<RectTransform>());
            itemLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            itemLabel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);

            template.SetActive(false);

            var dropdown = obj.AddComponent<TMP_Dropdown>();
            dropdown.template = tRect;
            dropdown.captionText = label.GetComponent<TextMeshProUGUI>();
            dropdown.itemText = itemLabel.GetComponent<TextMeshProUGUI>();

            return obj;
        }

        private static GameObject CreateInputField(Transform parent, string name, string defaultValue, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            obj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1);

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(obj.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            SetFullStretch(taRect);
            taRect.offsetMin = new Vector2(8, 0);
            taRect.offsetMax = new Vector2(-8, 0);
            textArea.AddComponent<RectMask2D>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            SetFullStretch(textObj.AddComponent<RectTransform>());
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = _textColor;

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            SetFullStretch(placeholder.AddComponent<RectTransform>());
            var ph = placeholder.AddComponent<TextMeshProUGUI>();
            ph.text = "...";
            ph.fontSize = 18;
            ph.color = new Color(1, 1, 1, 0.4f);
            ph.fontStyle = FontStyles.Italic;

            var input = obj.AddComponent<TMP_InputField>();
            input.textViewport = taRect;
            input.textComponent = tmp;
            input.placeholder = ph;
            input.text = defaultValue;

            return obj;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(GameObject obj, Vector2 min, Vector2 max)
        {
            var rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
            }
        }

        private static void ConnectSerializedField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        #endregion
    }
}
#endif
