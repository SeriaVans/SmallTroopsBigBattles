using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 直接創建中文字體 - 不通過菜單，直接執行
/// </summary>
[InitializeOnLoad]
public class DirectChineseFontCreator
{
    static DirectChineseFontCreator()
    {
        // 在編輯器啟動時自動檢查並創建字體
        EditorApplication.delayCall += CheckAndCreateFont;
    }

    private static void CheckAndCreateFont()
    {
        // 只在第一次啟動時執行
        if (EditorPrefs.GetBool("ChineseFontCreated", false))
        {
            return;
        }

        var fontPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        
        if (existing == null || existing.characterTable == null || existing.characterTable.Count < 100)
        {
            Debug.Log("[DirectChineseFontCreator] 檢測到需要創建中文字體，請執行 Tools > SLG Game > 快速修復繁體中文顯示");
        }
    }

    [MenuItem("Tools/SLG Game/立即創建並應用中文字體")]
    public static void CreateAndApplyFontNow()
    {
        CreateChineseFontAsset();
    }

    private static void CreateChineseFontAsset()
    {
        Debug.Log("=== 開始創建繁體中文字體 ===");

        var fontAssetPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        
        // 刪除舊的
        var old = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (old != null)
        {
            AssetDatabase.DeleteAsset(fontAssetPath);
            AssetDatabase.Refresh();
        }

        // 獲取字體
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/msjh.ttc");
        if (font == null)
        {
            Debug.LogError("找不到 msjh.ttc 字體文件！");
            EditorUtility.DisplayDialog("錯誤", "找不到中文字體文件 msjh.ttc！", "確定");
            return;
        }

        try
        {
            // 創建字體資源
            var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            if (fontAsset == null)
            {
                Debug.LogError("創建字體資源失敗！");
                return;
            }

            fontAsset.name = "ChineseFont_SDF";

            // 添加字符（使用較小的字符集以加快速度）
            var chars = new List<uint>();
            
            // ASCII
            for (uint i = 32; i <= 126; i++) chars.Add(i);
            
            // 繁體中文常用字符（只添加部分常用範圍以加快速度）
            // 0x4E00-0x62FF: 常用漢字
            for (uint i = 0x4E00; i <= 0x62FF; i++) chars.Add(i);
            // 0x6300-0x77FF: 更多常用漢字
            for (uint i = 0x6300; i <= 0x77FF; i++) chars.Add(i);
            // 0x7800-0x8CFF: 更多常用漢字
            for (uint i = 0x7800; i <= 0x8CFF; i++) chars.Add(i);
            // 0x8D00-0x9FFF: 更多常用漢字
            for (uint i = 0x8D00; i <= 0x9FFF; i++) chars.Add(i);
            
            // 標點符號
            for (uint i = 0x3000; i <= 0x303F; i++) chars.Add(i);
            
            // 全形字符
            for (uint i = 0xFF00; i <= 0xFFEF; i++) chars.Add(i);

            Debug.Log($"準備添加 {chars.Count} 個字符...");

            // 分批添加
            var charArray = chars.ToArray();
            int batchSize = 3000;
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
                    EditorUtility.DisplayProgressBar("創建中文字體", $"正在生成字體圖集... ({i + 1}/{totalBatches})", progress);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"批次 {i + 1} 失敗: {e.Message}");
                }
            }

            EditorUtility.ClearProgressBar();

            Debug.Log($"✓ 字體創建完成，包含 {fontAsset.characterTable.Count} 個字符");

            // 保存
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 應用字體
            ApplyFontToAllUI(fontAsset);

            EditorPrefs.SetBool("ChineseFontCreated", true);
            EditorUtility.DisplayDialog("完成", $"繁體中文字體已創建並應用！\n包含 {fontAsset.characterTable.Count} 個字符。", "確定");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"創建字體失敗: {e.Message}\n{e.StackTrace}");
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

