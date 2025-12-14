using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 快速修復繁體中文字體問題
/// </summary>
public class QuickChineseFontFix
{
    [MenuItem("Tools/SLG Game/快速修復繁體中文顯示")]
    public static void QuickFixChineseFont()
    {
        Debug.Log("=== 開始快速修復繁體中文顯示 ===");

        // 1. 獲取或創建中文字體
        var fontAsset = GetOrCreateChineseFont();
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("錯誤", "無法創建中文字體！請檢查系統是否安裝了中文字體。", "確定");
            return;
        }

        // 2. 應用字體到所有 UI 元素
        ApplyFontToAllUI(fontAsset);

        EditorUtility.DisplayDialog("完成", $"繁體中文字體已創建並應用到所有 UI 元素！\n字體包含 {fontAsset.characterTable.Count} 個字符。", "確定");
    }

    private static TMP_FontAsset GetOrCreateChineseFont()
    {
        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        
        // 檢查是否已存在
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (existing != null && existing.characterTable != null && existing.characterTable.Count > 0)
        {
            Debug.Log($"✓ 使用現有中文字體（包含 {existing.characterTable.Count} 個字符）");
            return existing;
        }

        // 刪除舊的（如果有問題）
        if (existing != null)
        {
            Debug.Log("刪除舊的字體資源...");
            AssetDatabase.DeleteAsset(fontAssetPath);
            AssetDatabase.Refresh();
        }

        // 獲取系統字體
        Font systemFont = null;
        
        // 優先使用已複製的 msjh.ttc
        var msjhPath = "Assets/_Project/Fonts/msjh.ttc";
        systemFont = AssetDatabase.LoadAssetAtPath<Font>(msjhPath);
        
        if (systemFont == null)
        {
            // 嘗試其他系統字體
            string[] fontPaths = {
                @"C:\Windows\Fonts\msjh.ttc",
                @"C:\Windows\Fonts\mingliu.ttc",
                @"C:\Windows\Fonts\kaiu.ttf"
            };

            var fontDir = "Assets/_Project/Fonts";
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
                    if (systemFont != null) break;
                }
            }
        }

        if (systemFont == null)
        {
            Debug.LogError("✗ 無法找到系統中文字體！");
            return null;
        }

        Debug.Log($"✓ 使用字體: {systemFont.name}");

        // 創建 TMP Font Asset
        try
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                systemFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic
            );

            if (fontAsset == null)
            {
                Debug.LogError("✗ 創建字體資源失敗！");
                return null;
            }

            fontAsset.name = "ChineseFont_SDF";

            // 添加字符集（分批添加以避免超時）
            var characterSet = BuildCharacterSet();
            var charArray = characterSet.ToArray();
            
            Debug.Log($"準備添加 {charArray.Length} 個字符...");
            
            // 分批添加
            int batchSize = 2000;
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
                    float progress = (float)(i + 1) / totalBatches;
                    EditorUtility.DisplayProgressBar("創建中文字體", $"正在添加字符... ({i + 1}/{totalBatches})", progress);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"批次 {i + 1} 添加失敗: {e.Message}");
                }
            }

            EditorUtility.ClearProgressBar();

            Debug.Log($"✓ 字體圖集生成完成，包含 {fontAsset.characterTable.Count} 個字符");

            // 保存
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return fontAsset;
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"✗ 創建字體失敗: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    private static HashSet<uint> BuildCharacterSet()
    {
        var charSet = new HashSet<uint>();

        // ASCII
        for (uint i = 32; i <= 126; i++) charSet.Add(i);

        // 繁體中文基本範圍 (0x4E00-0x9FFF)
        for (uint i = 0x4E00; i <= 0x9FFF; i++) charSet.Add(i);

        // 擴展 A (0x3400-0x4DBF)
        for (uint i = 0x3400; i <= 0x4DBF; i++) charSet.Add(i);

        // 標點符號 (0x3000-0x303F)
        for (uint i = 0x3000; i <= 0x303F; i++) charSet.Add(i);

        // 全形字符 (0xFF00-0xFFEF)
        for (uint i = 0xFF00; i <= 0xFFEF; i++) charSet.Add(i);

        return charSet;
    }

    private static void ApplyFontToAllUI(TMP_FontAsset font)
    {
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("找不到 MainCanvas");
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
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.scene);
        }
    }
}

