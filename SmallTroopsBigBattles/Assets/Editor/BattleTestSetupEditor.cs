using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Game.Battle;

/// <summary>
/// 戰鬥測試設置編輯器 - 自動設置測試 UI
/// </summary>
public class BattleTestSetupEditor : EditorWindow
{
    [MenuItem("Tools/SLG Game/Setup Battle Test UI")]
    public static void SetupBattleTestUI()
    {
        var mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas == null)
        {
            Debug.LogError("找不到 MainCanvas！");
            return;
        }

        // 創建測試面板
        CreateTestPanel(mainCanvas.transform);

        // 在 HUD 中添加測試按鈕
        AddTestButtonToHUD(mainCanvas.transform);

        Debug.Log("戰鬥測試 UI 設置完成！");
    }

    private static void CreateTestPanel(Transform parent)
    {
        // 檢查是否已存在
        var existing = parent.Find("BattleTestPanel");
        if (existing != null)
        {
            Debug.Log("BattleTestPanel 已存在，跳過創建");
            return;
        }

        var panelObj = new GameObject("BattleTestPanel");
        panelObj.transform.SetParent(parent.Find("NormalLayer"), false);

        // 添加 RectTransform
        var rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600, 400);

        // 添加背景
        var image = panelObj.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        // 添加 CanvasGroup
        panelObj.AddComponent<CanvasGroup>();

        // 添加腳本
        var panelType = System.Type.GetType("SmallTroopsBigBattles.UI.Battle.BattleTestPanel, Assembly-CSharp");
        if (panelType != null)
        {
            panelObj.AddComponent(panelType);
        }

        // 創建標題
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "戰鬥測試面板";
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        // 創建按鈕容器
        var buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(panelObj.transform, false);
        var buttonRect = buttonContainer.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0.7f);
        buttonRect.offsetMin = new Vector2(20, 20);
        buttonRect.offsetMax = new Vector2(-20, -20);

        var layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        // 創建按鈕
        CreateTestButton(buttonContainer.transform, "創建測試戰場", "createBattleButton");
        CreateTestButton(buttonContainer.transform, "添加士兵", "addSoldiersButton");
        CreateTestButton(buttonContainer.transform, "加速 (3x)", "speedUpButton");
        CreateTestButton(buttonContainer.transform, "暫停", "pauseButton");

        // 創建資訊文字
        var infoObj = new GameObject("InfoText");
        infoObj.transform.SetParent(panelObj.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0.7f);
        infoRect.anchorMax = new Vector2(1, 0.8f);
        infoRect.offsetMin = new Vector2(20, 0);
        infoRect.offsetMax = new Vector2(-20, 0);

        var infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "點擊按鈕開始測試";
        infoText.fontSize = 16;
        infoText.color = Color.white;

        // 設置為預設隱藏
        panelObj.SetActive(false);

        EditorUtility.SetDirty(panelObj);
        Debug.Log("BattleTestPanel 創建完成！");
    }

    private static void CreateTestButton(Transform parent, string buttonText, string buttonName)
    {
        var buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(parent, false);

        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f, 1f);

        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, 50);

        // 按鈕文字
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        SetStretchAll(textRect);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        // 設置按鈕顏色
        var colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.5f, 1f);
        button.colors = colors;
    }

    private static void AddTestButtonToHUD(Transform mainCanvas)
    {
        var hud = mainCanvas.Find("HUD/BottomButtonBar");
        if (hud == null)
        {
            Debug.LogWarning("找不到 HUD/BottomButtonBar");
            return;
        }

        // 檢查是否已存在
        if (hud.Find("TestBattleButton") != null)
        {
            Debug.Log("TestBattleButton 已存在");
            return;
        }

        var buttonObj = new GameObject("TestBattleButton");
        buttonObj.transform.SetParent(hud, false);

        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120, 100);

        // 按鈕文字
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        SetStretchAll(textRect);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "測試戰鬥";
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        // 綁定事件
        var gameHUD = mainCanvas.GetComponentInChildren<SmallTroopsBigBattles.UI.GameHUD>();
        if (gameHUD != null)
        {
            button.onClick.AddListener(() => gameHUD.OnTestBattleButtonClick());
        }

        EditorUtility.SetDirty(buttonObj);
        Debug.Log("測試按鈕添加到 HUD 完成！");
    }

    private static void SetStretchAll(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

