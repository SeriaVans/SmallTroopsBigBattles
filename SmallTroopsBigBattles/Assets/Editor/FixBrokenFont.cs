using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 修復損壞的字體資源
/// </summary>
public class FixBrokenFont
{
    [MenuItem("Tools/SLG Game/修復字體問題（刪除並重建）")]
    public static void FixFontIssue()
    {
        Debug.Log("=== 開始修復字體問題 ===");

        // 1. 刪除可能損壞的字體資源
        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        
        if (existing != null)
        {
            Debug.Log("刪除現有的字體資源（可能已損壞）...");
            AssetDatabase.DeleteAsset(fontAssetPath);
            
            // 也刪除相關的材質和紋理
            var matPath = "Assets/_Project/Fonts/ChineseFont_SDF Atlas Material.mat";
            if (File.Exists(matPath))
            {
                AssetDatabase.DeleteAsset(matPath);
            }
            
            var texPath = "Assets/_Project/Fonts/ChineseFont_SDF Atlas.png";
            if (File.Exists(texPath))
            {
                AssetDatabase.DeleteAsset(texPath);
            }
            
            AssetDatabase.Refresh();
            Debug.Log("✓ 已刪除舊的字體資源");
        }

        // 2. 暫時將所有文字設置為使用默認字體（避免錯誤）
        SetAllTextToDefaultFont();

        // 3. 創建新的字體資源
        CreateNewFontAsset();

        Debug.Log("=== 字體修復完成 ===");
    }

    private static void SetAllTextToDefaultFont()
    {
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("找不到 MainCanvas");
            return;
        }

        // 獲取默認的 TMP 字體
        var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            // 嘗試使用 LiberationSans
            var libSans = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name.Contains("Liberation") || f.name.Contains("Arial"));
            if (libSans != null)
            {
                var allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                int count = 0;
                foreach (var text in allTexts)
                {
                    if (text != null)
                    {
                        text.font = libSans;
                        count++;
                    }
                }
                Debug.Log($"✓ 已將 {count} 個文字元素設置為默認字體");
            }
        }
    }

    private static void CreateNewFontAsset()
    {
        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        
        // 獲取字體
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/msjh.ttc");
        if (font == null)
        {
            Debug.LogError("✗ 找不到 msjh.ttc 字體文件！");
            EditorUtility.DisplayDialog("錯誤", "找不到中文字體文件 msjh.ttc！", "確定");
            return;
        }

        try
        {
            Debug.Log("開始創建新的字體資源...");
            
            // 創建字體資源（使用較小的圖集以加快速度）
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font, 
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
                return;
            }

            fontAsset.name = "ChineseFont_SDF";

            // 添加字符集（先添加常用字符）
            var chars = new List<uint>();
            
            // ASCII
            for (uint i = 32; i <= 126; i++) chars.Add(i);
            
            // 繁體中文常用字符（分批添加）
            // 第一批：最常用的 2000 個字符
            for (uint i = 0x4E00; i <= 0x4E00 + 2000; i++) chars.Add(i);
            
            // 標點符號
            for (uint i = 0x3000; i <= 0x303F; i++) chars.Add(i);
            
            // 全形字符
            for (uint i = 0xFF00; i <= 0xFFEF; i++) chars.Add(i);

            Debug.Log($"準備添加 {chars.Count} 個字符...");

            // 添加字符
            var charArray = chars.ToArray();
            try
            {
                fontAsset.TryAddCharacters(charArray);
                Debug.Log($"✓ 字體圖集生成完成，包含 {fontAsset.characterTable.Count} 個字符");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"添加字符時出現警告: {e.Message}");
            }

            // 保存
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✓ 字體資源已保存");

            // 應用字體
            ApplyFontToAllUI(fontAsset);

            EditorUtility.DisplayDialog("完成", $"繁體中文字體已創建並應用！\n包含 {fontAsset.characterTable.Count} 個字符。", "確定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ 創建字體失敗: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("錯誤", $"創建字體失敗: {e.Message}", "確定");
        }
    }

    private static void ApplyFontToAllUI(TMP_FontAsset font)
    {
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null) return;

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

        if (canvas.scene.IsValid())
        {
            EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.scene);
        }
    }
}

