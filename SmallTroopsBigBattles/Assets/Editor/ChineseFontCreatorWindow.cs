using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 繁體中文字體創建器窗口
/// </summary>
public class ChineseFontCreatorWindow : EditorWindow
{
    private Font selectedFont;
    private TMP_FontAsset createdFontAsset;
    private bool isCreating = false;

    [MenuItem("Tools/SLG Game/中文字體創建器")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChineseFontCreatorWindow>("中文字體創建器");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("繁體中文字體創建器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 檢查現有字體
        var existingFontPath = "Assets/_Project/Fonts/ChineseFont_SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(existingFontPath);
        
        if (existing != null)
        {
            EditorGUILayout.HelpBox($"已存在中文字體資源\n包含 {existing.characterTable?.Count ?? 0} 個字符", MessageType.Info);
            
            if (GUILayout.Button("重新創建字體", GUILayout.Height(30)))
            {
                CreateFont();
            }
            
            if (GUILayout.Button("應用字體到所有 UI", GUILayout.Height(30)))
            {
                ApplyFontToAllUI(existing);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("尚未創建中文字體資源", MessageType.Warning);
            
            if (GUILayout.Button("創建繁體中文字體", GUILayout.Height(40)))
            {
                CreateFont();
            }
        }

        EditorGUILayout.Space();

        if (isCreating)
        {
            EditorGUILayout.HelpBox("正在創建字體，請稍候...", MessageType.Info);
        }

        // 顯示字體選擇
        EditorGUILayout.Space();
        GUILayout.Label("可用字體:", EditorStyles.boldLabel);
        
        var msjhFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/msjh.ttc");
        if (msjhFont != null)
        {
            EditorGUILayout.ObjectField("微軟正黑體", msjhFont, typeof(Font), false);
        }
        else
        {
            EditorGUILayout.HelpBox("未找到 msjh.ttc 字體文件", MessageType.Warning);
        }
    }

    private void CreateFont()
    {
        isCreating = true;
        Repaint();

        try
        {
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
                EditorUtility.DisplayDialog("錯誤", "找不到 msjh.ttc 字體文件！", "確定");
                isCreating = false;
                return;
            }

            // 創建字體資源
            var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("錯誤", "創建字體資源失敗！", "確定");
                isCreating = false;
                return;
            }

            fontAsset.name = "ChineseFont_SDF";

            // 添加字符集
            var chars = BuildCharacterSet();
            var charArray = chars.ToArray();
            
            Debug.Log($"準備添加 {charArray.Length} 個字符...");

            // 分批添加
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

            createdFontAsset = fontAsset;

            // 自動應用
            ApplyFontToAllUI(fontAsset);

            EditorUtility.DisplayDialog("完成", $"繁體中文字體已創建並應用！\n包含 {fontAsset.characterTable.Count} 個字符。", "確定");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"創建字體失敗: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("錯誤", $"創建字體失敗: {e.Message}", "確定");
        }
        finally
        {
            isCreating = false;
            Repaint();
        }
    }

    private static HashSet<uint> BuildCharacterSet()
    {
        var charSet = new HashSet<uint>();

        // ASCII
        for (uint i = 32; i <= 126; i++) charSet.Add(i);

        // 繁體中文 (0x4E00-0x9FFF)
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

        if (canvas.scene.IsValid())
        {
            EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.scene);
        }
    }
}

