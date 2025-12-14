using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 修復編輯器 - 修復 UI 顯示問題
/// </summary>
public class UIFixEditor : EditorWindow
{
    [MenuItem("Tools/SLG Game/Fix UI Display")]
    public static void FixUIDisplay()
    {
        var mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas == null)
        {
            Debug.LogError("找不到 MainCanvas！");
            return;
        }

        // 修復 Canvas 設置
        FixCanvas(mainCanvas);

        // 修復 HUD 設置
        FixHUD(mainCanvas);

        // 確保所有 UI 元素可見
        EnsureUIVisible(mainCanvas);

        Debug.Log("UI 顯示修復完成！");
    }

    private static void FixCanvas(GameObject canvas)
    {
        var canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 0;
        }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 確保 Canvas 是激活的
        canvas.SetActive(true);
    }

    private static void FixHUD(GameObject canvas)
    {
        var hud = canvas.transform.Find("HUD");
        if (hud == null) return;

        // 確保 HUD 激活
        hud.gameObject.SetActive(true);

        // 設置 HUD RectTransform
        var hudRect = hud.GetComponent<RectTransform>();
        if (hudRect != null)
        {
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
        }

        // 修復頂部資源列
        FixTopResourceBar(hud);

        // 修復底部按鈕列
        FixBottomButtonBar(hud);

        // 修復玩家資訊面板
        FixPlayerInfoPanel(hud);
    }

    private static void FixTopResourceBar(Transform hud)
    {
        var topBar = hud.Find("TopResourceBar");
        if (topBar == null) return;

        topBar.gameObject.SetActive(true);

        var rect = topBar.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 80);
        }

        var image = topBar.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        }

        // 修復文字元素
        FixResourceText(topBar, "CopperText", "銅錢: 0", new Color(1f, 0.84f, 0f));
        FixResourceText(topBar, "WoodText", "木材: 0", new Color(0.55f, 0.27f, 0.07f));
        FixResourceText(topBar, "StoneText", "石頭: 0", new Color(0.5f, 0.5f, 0.5f));
        FixResourceText(topBar, "FoodText", "糧草: 0", new Color(0.13f, 0.55f, 0.13f));
        FixResourceText(topBar, "SoldierCountText", "兵力: 0/5000", new Color(0.9f, 0.2f, 0.2f));
    }

    private static void FixResourceText(Transform parent, string name, string text, Color color)
    {
        var textObj = parent.Find(name);
        if (textObj == null) return;

        textObj.gameObject.SetActive(true);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
        }
    }

    private static void FixBottomButtonBar(Transform hud)
    {
        var bottomBar = hud.Find("BottomButtonBar");
        if (bottomBar == null) return;

        bottomBar.gameObject.SetActive(true);

        var rect = bottomBar.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 120);
        }

        var image = bottomBar.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        }

        // 修復按鈕
        FixButton(bottomBar, "TerritoryButton", "領地", new Color(0.2f, 0.6f, 0.2f));
        FixButton(bottomBar, "ArmyButton", "軍隊", new Color(0.8f, 0.2f, 0.2f));
        FixButton(bottomBar, "GeneralButton", "將領", new Color(0.6f, 0.3f, 0.8f));
        FixButton(bottomBar, "MapButton", "地圖", new Color(0.2f, 0.4f, 0.8f));
        FixButton(bottomBar, "SettingsButton", "設定", new Color(0.4f, 0.4f, 0.4f));
        FixButton(bottomBar, "TestBattleButton", "測試戰鬥", new Color(0.8f, 0.2f, 0.2f));
    }

    private static void FixButton(Transform parent, string name, string buttonText, Color color)
    {
        var buttonObj = parent.Find(name);
        if (buttonObj == null) return;

        buttonObj.gameObject.SetActive(true);

        var image = buttonObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        var button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            button.colors = colors;
        }

        // 修復按鈕文字
        var textObj = buttonObj.Find("Text");
        if (textObj != null)
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

    private static void FixPlayerInfoPanel(Transform hud)
    {
        var panel = hud.Find("PlayerInfoPanel");
        if (panel == null) return;

        panel.gameObject.SetActive(true);

        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        }

        FixInfoText(panel, "PlayerNameText", "測試玩家");
        FixInfoText(panel, "PlayerLevelText", "Lv1");
    }

    private static void FixInfoText(Transform parent, string name, string text)
    {
        var textObj = parent.Find(name);
        if (textObj == null) return;

        textObj.gameObject.SetActive(true);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = Color.white;
            tmp.fontSize = 20;
        }
    }

    private static void EnsureUIVisible(GameObject root)
    {
        // 確保所有 UI 元素可見
        var images = root.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.color.a < 0.1f)
            {
                var c = img.color;
                c.a = 1f;
                img.color = c;
            }
        }

        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in texts)
        {
            if (txt.color.a < 0.1f)
            {
                var c = txt.color;
                c.a = 1f;
                txt.color = c;
            }
        }

        // 確保 HUD 層級激活
        var hud = root.transform.Find("HUD");
        if (hud != null)
        {
            hud.gameObject.SetActive(true);
        }
    }
}

