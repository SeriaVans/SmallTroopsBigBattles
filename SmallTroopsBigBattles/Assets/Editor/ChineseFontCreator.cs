using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 繁體中文字體創建器 - 創建支持繁體中文的 TMP Font Asset
/// </summary>
public class ChineseFontCreator
{
    [MenuItem("Tools/SLG Game/創建繁體中文字體")]
    public static void CreateChineseFont()
    {
        Debug.Log("=== 開始創建繁體中文字體 ===");

        // 1. 確保字體目錄存在
        var fontDir = "Assets/_Project/Fonts";
        if (!Directory.Exists(fontDir))
        {
            Directory.CreateDirectory(fontDir);
            AssetDatabase.Refresh();
        }

        // 2. 檢查是否已存在
        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (existing != null)
        {
            Debug.Log("✓ 中文字體已存在，將重新生成...");
            AssetDatabase.DeleteAsset(fontAssetPath);
            AssetDatabase.Refresh();
        }

        // 3. 獲取系統中文字體
        Font systemFont = GetSystemChineseFont(fontDir);
        if (systemFont == null)
        {
            Debug.LogError("✗ 無法找到系統中文字體！");
            EditorUtility.DisplayDialog("錯誤", "無法找到系統中文字體。請確保 Windows 系統已安裝中文字體（如微軟正黑體、新細明體等）。", "確定");
            return;
        }

        Debug.Log($"✓ 使用字體: {systemFont.name}");

        // 4. 創建 TMP Font Asset
        try
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                systemFont,
                90,                          // 字體大小
                9,                           // 填充
                GlyphRenderMode.SDFAA,       // SDF 渲染模式
                1024,                        // 圖集寬度
                1024,                        // 圖集高度
                AtlasPopulationMode.Dynamic  // 動態圖集
            );

            if (fontAsset == null)
            {
                Debug.LogError("✗ 創建字體資源失敗！");
                return;
            }

            fontAsset.name = "ChineseFont_SDF";

            // 5. 添加字符集
            var characterSet = BuildChineseCharacterSet();
            Debug.Log($"準備添加 {characterSet.Count} 個字符...");

            // 分批添加字符（避免一次性添加太多導致超時）
            var charArray = characterSet.ToArray();
            int batchSize = 1000;
            int totalBatches = (charArray.Length + batchSize - 1) / batchSize;

            for (int i = 0; i < totalBatches; i++)
            {
                int start = i * batchSize;
                int end = Mathf.Min(start + batchSize, charArray.Length);
                var batch = new uint[end - start];
                System.Array.Copy(charArray, start, batch, 0, end - start);

                try
                {
                    fontAsset.TryAddCharacters(batch);
                    Debug.Log($"已添加批次 {i + 1}/{totalBatches} ({batch.Length} 個字符)");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"批次 {i + 1} 添加失敗: {e.Message}");
                }

                // 更新進度條
                EditorUtility.DisplayProgressBar(
                    "創建中文字體",
                    $"正在添加字符... ({i + 1}/{totalBatches})",
                    (float)(i + 1) / totalBatches
                );
            }

            EditorUtility.ClearProgressBar();

            Debug.Log($"✓ 字體圖集生成完成，包含 {fontAsset.characterTable.Count} 個字符");

            // 6. 保存字體資源
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✓ 繁體中文字體已創建: {fontAssetPath}");

            // 7. 應用字體到所有 UI 元素
            ApplyFontToAllUI(fontAsset);

            EditorUtility.DisplayDialog("完成", "繁體中文字體已創建並應用到所有 UI 元素！", "確定");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"✗ 創建字體失敗: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("錯誤", $"創建字體失敗: {e.Message}", "確定");
        }
    }

    /// <summary>
    /// 獲取系統中文字體
    /// </summary>
    private static Font GetSystemChineseFont(string fontDir)
    {
        // Windows 系統字體路徑
        string[] fontPaths = {
            @"C:\Windows\Fonts\msjh.ttc",      // 微軟正黑體
            @"C:\Windows\Fonts\mingliu.ttc",   // 新細明體
            @"C:\Windows\Fonts\kaiu.ttf",     // 標楷體
            @"C:\Windows\Fonts\msjhbd.ttc",    // 微軟正黑體 Bold
        };

        foreach (var path in fontPaths)
        {
            if (File.Exists(path))
            {
                var fileName = Path.GetFileName(path);
                var projectPath = Path.Combine(fontDir, fileName);

                // 複製字體到項目
                if (!File.Exists(projectPath))
                {
                    try
                    {
                        File.Copy(path, projectPath);
                        AssetDatabase.Refresh();
                        Debug.Log($"✓ 已複製字體: {fileName}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"複製字體失敗 {fileName}: {e.Message}");
                        continue;
                    }
                }

                // 導入字體
                var fontImporter = AssetImporter.GetAtPath(projectPath) as TrueTypeFontImporter;
                if (fontImporter != null)
                {
                    fontImporter.fontSize = 16;
                    fontImporter.characterSpacing = 0;
                    fontImporter.fontRenderingMode = FontRenderingMode.Smooth;
                    EditorUtility.SetDirty(fontImporter);
                    fontImporter.SaveAndReimport();
                }

                var font = AssetDatabase.LoadAssetAtPath<Font>(projectPath);
                if (font != null)
                {
                    Debug.Log($"✓ 找到系統字體: {fileName}");
                    return font;
                }
            }
        }

        // 如果找不到系統字體，使用 LiberationSans 作為備用
        var fallbackFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
        if (fallbackFont != null)
        {
            Debug.LogWarning("⚠ 未找到系統中文字體，使用 LiberationSans 作為備用（可能不支持中文）");
            return fallbackFont;
        }

        return null;
    }

    /// <summary>
    /// 構建繁體中文字符集
    /// </summary>
    private static HashSet<uint> BuildChineseCharacterSet()
    {
        var charSet = new HashSet<uint>();

        // 1. 基本 ASCII (32-126)
        for (uint i = 32; i <= 126; i++)
        {
            charSet.Add(i);
        }

        // 2. 繁體中文常用字符 (CJK Unified Ideographs)
        // 基本範圍: 0x4E00-0x9FFF (約 20,000 個字符)
        // 為了性能，我們只添加常用範圍
        for (uint i = 0x4E00; i <= 0x9FFF; i++)
        {
            charSet.Add(i);
        }

        // 3. 擴展 A: 0x3400-0x4DBF
        for (uint i = 0x3400; i <= 0x4DBF; i++)
        {
            charSet.Add(i);
        }

        // 4. 常用標點符號
        // CJK Symbols and Punctuation: 0x3000-0x303F
        for (uint i = 0x3000; i <= 0x303F; i++)
        {
            charSet.Add(i);
        }

        // 5. 全形字符
        // Halfwidth and Fullwidth Forms: 0xFF00-0xFFEF
        for (uint i = 0xFF00; i <= 0xFFEF; i++)
        {
            charSet.Add(i);
        }

        // 6. 常用數字和符號
        // 添加一些常用的 Unicode 字符
        uint[] commonChars = {
            0x2018, 0x2019, 0x201C, 0x201D, // 引號
            0x2026, // 省略號
            0x3001, 0x3002, // 頓號、句號
            0xFF01, 0xFF0C, 0xFF1A, 0xFF1B, // 全形標點
        };

        foreach (var ch in commonChars)
        {
            charSet.Add(ch);
        }

        Debug.Log($"✓ 字符集構建完成，共 {charSet.Count} 個字符");
        return charSet;
    }

    /// <summary>
    /// 將字體應用到所有 UI 元素
    /// </summary>
    private static void ApplyFontToAllUI(TMP_FontAsset font)
    {
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("找不到 MainCanvas，無法應用字體");
            return;
        }

        var allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        int count = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.font = font;
                count++;
            }
        }

        Debug.Log($"✓ 已應用字體到 {count} 個文字元素");

        // 標記場景為已修改
        if (canvas.scene.IsValid())
        {
            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(canvas.scene);
        }
    }
}

