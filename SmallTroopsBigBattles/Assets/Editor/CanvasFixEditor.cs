using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas 修復編輯器 - 修復 Canvas 和 UI 元素的位置、大小和顯示問題
/// </summary>
public class CanvasFixEditor
{
    [MenuItem("Tools/SLG Game/修復 Canvas 顯示問題")]
    public static void FixCanvasDisplay()
    {
        Debug.Log("=== 開始修復 Canvas 顯示問題 ===");

        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogError("找不到 MainCanvas！");
            return;
        }

        // 1. 修復 Canvas RectTransform
        var canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.localScale = Vector3.one;
            canvasRect.anchoredPosition = Vector2.zero;
            Debug.Log("✓ Canvas RectTransform 已修復");
        }

        // 2. 確保 Canvas 設置正確
        var canvasComp = canvas.GetComponent<Canvas>();
        if (canvasComp != null)
        {
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.enabled = true;
            Debug.Log("✓ Canvas Render Mode 已設置為 ScreenSpaceOverlay");
        }

        // 3. 修復 Canvas Scaler
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            Debug.Log("✓ Canvas Scaler 已修復");
        }

        // 4. 修復 HUD
        var hud = canvas.transform.Find("HUD");
        if (hud != null)
        {
            hud.gameObject.SetActive(true);
            var hudRect = hud.GetComponent<RectTransform>();
            if (hudRect != null)
            {
                hudRect.anchorMin = Vector2.zero;
                hudRect.anchorMax = Vector2.one;
                hudRect.offsetMin = Vector2.zero;
                hudRect.offsetMax = Vector2.zero;
                hudRect.localScale = Vector3.one;
                hudRect.anchoredPosition = Vector2.zero;
                Debug.Log("✓ HUD RectTransform 已修復");
            }

            // 確保所有 HUD 子元素可見
            EnsureChildrenActive(hud.gameObject);
        }

        // 5. 修復頂部資源列
        var topBar = canvas.transform.Find("HUD/TopResourceBar");
        if (topBar != null)
        {
            var rect = topBar.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(0, 80);
                rect.localScale = Vector3.one;
            }
            Debug.Log("✓ 頂部資源列已修復");
        }

        // 6. 修復底部按鈕列
        var bottomBar = canvas.transform.Find("HUD/BottomButtonBar");
        if (bottomBar != null)
        {
            var rect = bottomBar.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(0, 120);
                rect.localScale = Vector3.one;
            }
            Debug.Log("✓ 底部按鈕列已修復");
        }

        // 7. 修復玩家資訊面板
        var playerPanel = canvas.transform.Find("HUD/PlayerInfoPanel");
        if (playerPanel != null)
        {
            var rect = playerPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(10, -10);
                rect.sizeDelta = new Vector2(200, 60);
                rect.localScale = Vector3.one;
            }
            Debug.Log("✓ 玩家資訊面板已修復");
        }

        // 8. 確保所有 UI 元素有正確的顏色
        EnsureUIVisible(canvas);

        EditorUtility.SetDirty(canvas);
        Debug.Log("=== Canvas 顯示問題修復完成 ===");
    }

    private static void EnsureChildrenActive(GameObject obj)
    {
        obj.SetActive(true);
        foreach (Transform child in obj.transform)
        {
            EnsureChildrenActive(child.gameObject);
        }
    }

    private static void EnsureUIVisible(GameObject root)
    {
        // 確保所有 Image 組件可見
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

        // 確保所有 TextMeshProUGUI 組件可見
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
    }
}

