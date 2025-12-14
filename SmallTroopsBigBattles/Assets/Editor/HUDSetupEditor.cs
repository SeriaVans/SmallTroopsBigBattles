using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.UI;

/// <summary>
/// HUD 設置編輯器 - 自動設置 HUD UI 佈局和引用連接
/// </summary>
public class HUDSetupEditor : EditorWindow
{
    [MenuItem("Tools/SLG Game/Setup HUD")]
    public static void SetupHUD()
    {
        // 找到 HUD 物件
        var hud = GameObject.Find("MainCanvas/HUD");
        if (hud == null)
        {
            Debug.LogError("找不到 MainCanvas/HUD 物件！");
            return;
        }

        // 設置 HUD RectTransform
        var hudRect = hud.GetComponent<RectTransform>();
        if (hudRect == null)
        {
            hudRect = hud.AddComponent<RectTransform>();
        }
        SetStretchAll(hudRect);

        // 設置頂部資源列
        SetupTopResourceBar();

        // 設置底部按鈕列
        SetupBottomButtonBar();

        // 設置玩家資訊面板
        SetupPlayerInfoPanel();

        // 連接 GameHUD 腳本引用
        ConnectGameHUDReferences();

        Debug.Log("HUD 設置完成！");
        EditorUtility.SetDirty(hud);
    }

    private static void SetupTopResourceBar()
    {
        var topBar = GameObject.Find("MainCanvas/HUD/TopResourceBar");
        if (topBar == null) return;

        var rect = topBar.GetComponent<RectTransform>();
        if (rect == null) rect = topBar.AddComponent<RectTransform>();

        // 設置錨點到頂部
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 80);

        // 設置背景顏色
        var image = topBar.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        }

        // 設置 Layout Group
        var layout = topBar.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 30;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
        }

        // 設置資源文字
        SetupResourceText("MainCanvas/HUD/TopResourceBar/CopperText", "銅錢: 0", new Color(1f, 0.84f, 0f));
        SetupResourceText("MainCanvas/HUD/TopResourceBar/WoodText", "木材: 0", new Color(0.55f, 0.27f, 0.07f));
        SetupResourceText("MainCanvas/HUD/TopResourceBar/StoneText", "石頭: 0", new Color(0.5f, 0.5f, 0.5f));
        SetupResourceText("MainCanvas/HUD/TopResourceBar/FoodText", "糧草: 0", new Color(0.13f, 0.55f, 0.13f));
        SetupResourceText("MainCanvas/HUD/TopResourceBar/SoldierCountText", "兵力: 0/5000", new Color(0.9f, 0.2f, 0.2f));
    }

    private static void SetupResourceText(string path, string defaultText, Color color)
    {
        var obj = GameObject.Find(path);
        if (obj == null) return;

        var text = obj.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = defaultText;
            text.color = color;
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
        }

        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(150, 60);
        }

        // 添加 LayoutElement
        var layoutElement = obj.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = obj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 150;
        layoutElement.preferredHeight = 60;
    }

    private static void SetupBottomButtonBar()
    {
        var bottomBar = GameObject.Find("MainCanvas/HUD/BottomButtonBar");
        if (bottomBar == null) return;

        var rect = bottomBar.GetComponent<RectTransform>();
        if (rect == null) rect = bottomBar.AddComponent<RectTransform>();

        // 設置錨點到底部
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 120);

        // 設置背景顏色
        var image = bottomBar.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        }

        // 設置 Layout Group
        var layout = bottomBar.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 20;
            layout.padding = new RectOffset(50, 50, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        // 設置按鈕
        SetupButton("MainCanvas/HUD/BottomButtonBar/TerritoryButton", "領地", new Color(0.2f, 0.6f, 0.2f));
        SetupButton("MainCanvas/HUD/BottomButtonBar/ArmyButton", "軍隊", new Color(0.8f, 0.2f, 0.2f));
        SetupButton("MainCanvas/HUD/BottomButtonBar/GeneralButton", "將領", new Color(0.6f, 0.3f, 0.8f));
        SetupButton("MainCanvas/HUD/BottomButtonBar/MapButton", "地圖", new Color(0.2f, 0.4f, 0.8f));
        SetupButton("MainCanvas/HUD/BottomButtonBar/SettingsButton", "設定", new Color(0.4f, 0.4f, 0.4f));
    }

    private static void SetupButton(string path, string buttonText, Color color)
    {
        var obj = GameObject.Find(path);
        if (obj == null) return;

        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(120, 100);
        }

        // 設置按鈕背景顏色
        var image = obj.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        // 設置按鈕
        var button = obj.GetComponent<Button>();
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            button.colors = colors;
        }

        // 添加 LayoutElement
        var layoutElement = obj.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = obj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 120;
        layoutElement.preferredHeight = 100;

        // 創建或獲取按鈕文字
        var textObj = obj.transform.Find("Text");
        if (textObj == null)
        {
            var newTextObj = new GameObject("Text");
            newTextObj.transform.SetParent(obj.transform, false);

            var textRect = newTextObj.AddComponent<RectTransform>();
            SetStretchAll(textRect);

            var tmp = newTextObj.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.color = Color.white;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = buttonText;
                tmp.color = Color.white;
                tmp.fontSize = 24;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    private static void SetupPlayerInfoPanel()
    {
        var panel = GameObject.Find("MainCanvas/HUD/PlayerInfoPanel");
        if (panel == null) return;

        var rect = panel.GetComponent<RectTransform>();
        if (rect == null) rect = panel.AddComponent<RectTransform>();

        // 設置錨點到左上角
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -90);
        rect.sizeDelta = new Vector2(200, 60);

        // 設置背景顏色
        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        }

        // 創建玩家名稱文字
        CreatePlayerInfoText(panel.transform, "PlayerNameText", "測試玩家", new Vector2(10, -10));
        CreatePlayerInfoText(panel.transform, "PlayerLevelText", "Lv1", new Vector2(150, -10));
    }

    private static void CreatePlayerInfoText(Transform parent, string name, string defaultText, Vector2 position)
    {
        var existing = parent.Find(name);
        GameObject textObj;

        if (existing == null)
        {
            textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
        }
        else
        {
            textObj = existing.gameObject;
        }

        var rect = textObj.GetComponent<RectTransform>();
        if (rect == null) rect = textObj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 40);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();

        tmp.text = defaultText;
        tmp.color = Color.white;
        tmp.fontSize = 20;
    }

    private static void ConnectGameHUDReferences()
    {
        var hud = GameObject.Find("MainCanvas/HUD");
        if (hud == null) return;

        var gameHUD = hud.GetComponent<GameHUD>();
        if (gameHUD == null)
        {
            Debug.LogWarning("找不到 GameHUD 組件！");
            return;
        }

        // 使用 SerializedObject 連接引用
        var serializedObject = new SerializedObject(gameHUD);

        // 連接玩家資訊
        ConnectReference(serializedObject, "playerNameText", "MainCanvas/HUD/PlayerInfoPanel/PlayerNameText");
        ConnectReference(serializedObject, "playerLevelText", "MainCanvas/HUD/PlayerInfoPanel/PlayerLevelText");

        // 連接資源文字
        ConnectReference(serializedObject, "copperText", "MainCanvas/HUD/TopResourceBar/CopperText");
        ConnectReference(serializedObject, "woodText", "MainCanvas/HUD/TopResourceBar/WoodText");
        ConnectReference(serializedObject, "stoneText", "MainCanvas/HUD/TopResourceBar/StoneText");
        ConnectReference(serializedObject, "foodText", "MainCanvas/HUD/TopResourceBar/FoodText");
        ConnectReference(serializedObject, "soldierCountText", "MainCanvas/HUD/TopResourceBar/SoldierCountText");

        // 連接按鈕
        ConnectButtonReference(serializedObject, "territoryButton", "MainCanvas/HUD/BottomButtonBar/TerritoryButton");
        ConnectButtonReference(serializedObject, "armyButton", "MainCanvas/HUD/BottomButtonBar/ArmyButton");
        ConnectButtonReference(serializedObject, "generalButton", "MainCanvas/HUD/BottomButtonBar/GeneralButton");
        ConnectButtonReference(serializedObject, "mapButton", "MainCanvas/HUD/BottomButtonBar/MapButton");
        ConnectButtonReference(serializedObject, "settingsButton", "MainCanvas/HUD/BottomButtonBar/SettingsButton");

        serializedObject.ApplyModifiedProperties();
        Debug.Log("GameHUD 引用連接完成！");
    }

    private static void ConnectReference(SerializedObject serializedObject, string propertyName, string path)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    property.objectReferenceValue = tmp;
                }
            }
        }
    }

    private static void ConnectButtonReference(SerializedObject serializedObject, string propertyName, string path)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                var button = obj.GetComponent<Button>();
                if (button != null)
                {
                    property.objectReferenceValue = button;
                }
            }
        }
    }

    private static void SetStretchAll(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}

