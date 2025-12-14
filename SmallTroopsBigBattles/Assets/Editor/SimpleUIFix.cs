using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;

/// <summary>
/// 簡化版 UI 修復 - 直接修復 UI 顯示和字體問題
/// </summary>
public class SimpleUIFix
{
    [MenuItem("Tools/SLG Game/快速修復 UI")]
    public static void QuickFixUI()
    {
        Debug.Log("=== 開始快速修復 UI ===");

        // 1. 修復 Canvas
        var canvas = GameObject.Find("MainCanvas");
        if (canvas != null)
        {
            canvas.SetActive(true);
            
            var canvasComp = canvas.GetComponent<Canvas>();
            if (canvasComp != null)
            {
                canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasComp.enabled = true;
            }

            // 2. 確保 HUD 可見
            var hud = canvas.transform.Find("HUD");
            if (hud != null)
            {
                hud.gameObject.SetActive(true);
                EnsureAllChildrenActive(hud.gameObject);
            }

            // 3. 創建並應用中文字體
            var chineseFont = GetOrCreateChineseFont();
            if (chineseFont != null)
            {
                ApplyFontToAllText(canvas, chineseFont);
            }

            Debug.Log("✓ UI 修復完成");
        }
        else
        {
            Debug.LogError("✗ 找不到 MainCanvas！");
        }

        // 4. 確保 EventSystem 存在
        var eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✓ 創建了 EventSystem");
        }
    }

    private static void EnsureAllChildrenActive(GameObject obj)
    {
        obj.SetActive(true);
        foreach (Transform child in obj.transform)
        {
            EnsureAllChildrenActive(child.gameObject);
        }
    }

    private static TMP_FontAsset GetOrCreateChineseFont()
    {
        var fontPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (existing != null)
        {
            Debug.Log("✓ 使用現有中文字體");
            return existing;
        }

        // 創建目錄
        var fontDir = "Assets/_Project/Fonts";
        if (!Directory.Exists(fontDir))
        {
            Directory.CreateDirectory(fontDir);
            AssetDatabase.Refresh();
        }

        // 嘗試使用系統字體
        Font systemFont = null;
        string[] fontPaths = {
            @"C:\Windows\Fonts\msjh.ttc",
            @"C:\Windows\Fonts\mingliu.ttc",
            @"C:\Windows\Fonts\kaiu.ttf"
        };

        foreach (var path in fontPaths)
        {
            if (File.Exists(path))
            {
                var fileName = Path.GetFileName(path);
                var projectPath = Path.Combine(fontDir, fileName);
                
                if (!File.Exists(projectPath))
                {
                    File.Copy(path, projectPath);
                    AssetDatabase.Refresh();
                }

                systemFont = AssetDatabase.LoadAssetAtPath<Font>(projectPath);
                if (systemFont != null)
                {
                    Debug.Log($"✓ 找到系統字體: {fileName}");
                    break;
                }
            }
        }

        // 如果找不到，使用 LiberationSans
        if (systemFont == null)
        {
            systemFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            Debug.LogWarning("⚠ 使用 LiberationSans 作為備用字體");
        }

        if (systemFont == null)
        {
            Debug.LogError("✗ 無法創建字體！");
            return null;
        }

        // 創建 TMP Font Asset
        try
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(systemFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            if (fontAsset != null)
            {
                fontAsset.name = "ChineseFont_SDF";
                
                // 添加基本字符集
                var chars = new System.Collections.Generic.HashSet<uint>();
                for (uint i = 32; i <= 126; i++) chars.Add(i); // ASCII
                for (uint i = 0x4E00; i <= 0x9FFF; i++) chars.Add(i); // 中文
                for (uint i = 0x3000; i <= 0x303F; i++) chars.Add(i); // 標點
                for (uint i = 0xFF00; i <= 0xFFEF; i++) chars.Add(i); // 全形

                try
                {
                    fontAsset.TryAddCharacters(chars.ToArray());
                }
                catch { }

                AssetDatabase.CreateAsset(fontAsset, fontPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"✓ 創建中文字體: {fontPath}");
                return fontAsset;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ 創建字體失敗: {e.Message}");
        }

        return null;
    }

    private static void ApplyFontToAllText(GameObject root, TMP_FontAsset font)
    {
        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        int count = 0;
        foreach (var text in texts)
        {
            if (text != null && text.font != font)
            {
                text.font = font;
                text.enabled = true;
                count++;
            }
        }
        Debug.Log($"✓ 已應用字體到 {count} 個文字元素");
    }
}

