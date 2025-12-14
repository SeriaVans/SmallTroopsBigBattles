using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;

/// <summary>
/// UI 修復和字體設置編輯器 - 全面修復 UI 顯示問題並設置繁體中文字體
/// </summary>
public class UIFixAndFontSetup : EditorWindow
{
    [MenuItem("Tools/SLG Game/完整修復 UI 和字體")]
    public static void CompleteUIFix()
    {
        Debug.Log("=== 開始完整 UI 修復 ===");

        // 1. 修復 Input System 設置
        FixInputSystemSettings();

        // 2. 創建或獲取繁體中文字體
        var chineseFont = CreateOrGetChineseFont();

        // 3. 修復 Canvas 設置
        var canvas = GameObject.Find("MainCanvas");
        if (canvas != null)
        {
            FixCanvas(canvas);
            FixHUD(canvas, chineseFont);
        }

        // 4. 修復 EventSystem
        FixEventSystem();

        Debug.Log("=== UI 修復完成 ===");
        EditorUtility.DisplayDialog("完成", "UI 修復完成！請運行遊戲查看效果。", "確定");
    }

    /// <summary>
    /// 修復 Input System 設置 - 改為 Both 模式以兼容舊的 StandaloneInputModule
    /// </summary>
    private static void FixInputSystemSettings()
    {
        // 讀取 ProjectSettings.asset
        var projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        if (File.Exists(projectSettingsPath))
        {
            var content = File.ReadAllText(projectSettingsPath);
            
            // 查找 activeInputHandler 設置
            if (content.Contains("activeInputHandler:"))
            {
                // 替換為 Both (值為 2)
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"activeInputHandler:\s*\d+",
                    "activeInputHandler: 2"
                );
                
                File.WriteAllText(projectSettingsPath, content);
                Debug.Log("✓ Input System 設置已修復為 Both 模式");
            }
            else
            {
                // 如果不存在，添加設置
                var insertPos = content.IndexOf("m_BuildTargetBatching:");
                if (insertPos > 0)
                {
                    content = content.Insert(insertPos, "  activeInputHandler: 2\n");
                    File.WriteAllText(projectSettingsPath, content);
                    Debug.Log("✓ Input System 設置已添加");
                }
            }
            
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 創建或獲取支持繁體中文的 TMP Font Asset
    /// </summary>
    private static TMP_FontAsset CreateOrGetChineseFont()
    {
        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        
        // 檢查是否已存在
        var existingFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (existingFont != null)
        {
            Debug.Log("✓ 使用現有的中文字體");
            return existingFont;
        }

        // 創建字體目錄
        var fontDir = "Assets/_Project/Fonts";
        if (!Directory.Exists(fontDir))
        {
            Directory.CreateDirectory(fontDir);
        }

        // 嘗試使用系統字體（Windows 上的微軟正黑體或新細明體）
        Font systemFont = null;
        
        // Windows 系統字體路徑
        string[] fontPaths = new string[]
        {
            @"C:\Windows\Fonts\msjh.ttc",      // 微軟正黑體
            @"C:\Windows\Fonts\mingliu.ttc",    // 新細明體
            @"C:\Windows\Fonts\kaiu.ttf",      // 標楷體
        };

        foreach (var path in fontPaths)
        {
            if (File.Exists(path))
            {
                // 複製字體到項目
                var fontFileName = Path.GetFileName(path);
                var projectFontPath = Path.Combine(fontDir, fontFileName);
                
                if (!File.Exists(projectFontPath))
                {
                    File.Copy(path, projectFontPath);
                    AssetDatabase.Refresh();
                }

                // 導入字體
                var fontImporter = AssetImporter.GetAtPath(projectFontPath) as TrueTypeFontImporter;
                if (fontImporter != null)
                {
                    fontImporter.fontSize = 16;
                    fontImporter.characterSpacing = 0;
                    fontImporter.fontRenderingMode = FontRenderingMode.Smooth;
                    EditorUtility.SetDirty(fontImporter);
                    fontImporter.SaveAndReimport();
                }

                systemFont = AssetDatabase.LoadAssetAtPath<Font>(projectFontPath);
                if (systemFont != null)
                {
                    Debug.Log($"✓ 找到系統字體: {fontFileName}");
                    break;
                }
            }
        }

        // 如果找不到系統字體，使用 LiberationSans 作為備用
        if (systemFont == null)
        {
            systemFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            Debug.LogWarning("⚠ 未找到系統中文字體，使用 LiberationSans 作為備用");
        }

        if (systemFont == null)
        {
            Debug.LogError("✗ 無法創建字體！");
            return null;
        }

        // 創建 TMP Font Asset
        var fontAsset = TMP_FontAsset.CreateFontAsset(systemFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        if (fontAsset != null)
        {
            fontAsset.name = "ChineseFont_SDF";
            
            // 設置字體包含繁體中文字符集
            var characterSet = new System.Collections.Generic.HashSet<uint>();
            
            // 添加基本 ASCII
            for (uint i = 32; i <= 126; i++)
            {
                characterSet.Add(i);
            }
            
            // 添加繁體中文常用字符範圍
            // CJK Unified Ideographs (基本範圍)
            for (uint i = 0x4E00; i <= 0x9FFF; i++)
            {
                characterSet.Add(i);
            }
            
            // 添加常用標點符號
            for (uint i = 0x3000; i <= 0x303F; i++)
            {
                characterSet.Add(i);
            }
            
            // 添加全形字符
            for (uint i = 0xFF00; i <= 0xFFEF; i++)
            {
                characterSet.Add(i);
            }

            // 生成字體圖集（這可能需要一些時間）
            try
            {
                fontAsset.TryAddCharacters(characterSet.ToArray());
                Debug.Log($"✓ 字體圖集生成完成，包含 {fontAsset.characterTable.Count} 個字符");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠ 字體圖集生成部分失敗: {e.Message}");
            }

            // 保存字體資源
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✓ 繁體中文字體已創建: {fontAssetPath}");
            return fontAsset;
        }

        return null;
    }

    /// <summary>
    /// 修復 Canvas 設置
    /// </summary>
    private static void FixCanvas(GameObject canvas)
    {
        canvas.SetActive(true);

        var canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 0;
            canvasComponent.enabled = true;
        }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        var raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = true;
        }
    }

    /// <summary>
    /// 修復 HUD 並應用字體
    /// </summary>
    private static void FixHUD(GameObject canvas, TMP_FontAsset chineseFont)
    {
        var hud = canvas.transform.Find("HUD");
        if (hud == null)
        {
            Debug.LogError("✗ 找不到 HUD！");
            return;
        }

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

        // 應用字體到所有文字元素
        if (chineseFont != null)
        {
            ApplyFontToAllText(hud.gameObject, chineseFont);
        }

        // 確保所有子元素可見
        EnsureChildrenVisible(hud.gameObject);
    }

    /// <summary>
    /// 將字體應用到所有 TextMeshProUGUI 組件
    /// </summary>
    private static void ApplyFontToAllText(GameObject root, TMP_FontAsset font)
    {
        var allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        int count = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.font = font;
                text.enabled = true;
                text.gameObject.SetActive(true);
                count++;
            }
        }
        Debug.Log($"✓ 已應用字體到 {count} 個文字元素");
    }

    /// <summary>
    /// 確保所有子元素可見
    /// </summary>
    private static void EnsureChildrenVisible(GameObject obj)
    {
        obj.SetActive(true);
        foreach (Transform child in obj.transform)
        {
            EnsureChildrenVisible(child.gameObject);
        }
    }

    /// <summary>
    /// 修復 EventSystem
    /// </summary>
    private static void FixEventSystem()
    {
        var eventSystem = GameObject.Find("EventSystem");
        if (eventSystem != null)
        {
            eventSystem.SetActive(true);
            Debug.Log("✓ EventSystem 已激活");
        }
        else
        {
            Debug.LogWarning("⚠ EventSystem 不存在，將在運行時自動創建");
        }
    }
}

