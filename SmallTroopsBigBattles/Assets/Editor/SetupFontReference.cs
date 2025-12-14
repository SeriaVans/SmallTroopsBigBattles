using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// 設置字體引用 - 確保 FontFixerOnStart 組件正確引用字體資源
/// </summary>
public class SetupFontReference
{
    [MenuItem("Tools/SLG Game/設置字體引用")]
    public static void SetupFont()
    {
        // 加載字體資源
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/ChineseFont_SDF.asset");
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("錯誤", "找不到字體資源！請先執行 Tools > SLG Game > 修復字體問題（刪除並重建）", "確定");
            return;
        }

        // 查找 FontFixerOnStart 組件
        var fontFixer = GameObject.FindObjectOfType<SmallTroopsBigBattles.Core.FontFixerOnStart>();
        if (fontFixer == null)
        {
            // 如果不存在，創建一個
            var go = new GameObject("[FontFixerOnStart]");
            fontFixer = go.AddComponent<SmallTroopsBigBattles.Core.FontFixerOnStart>();
            Debug.Log("創建了新的 FontFixerOnStart 組件");
        }

        // 使用反射設置字體引用
        var field = typeof(SmallTroopsBigBattles.Core.FontFixerOnStart).GetField("chineseFontAsset", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(fontFixer, fontAsset);
            EditorUtility.SetDirty(fontFixer);
            Debug.Log($"✓ 已設置字體引用到 FontFixerOnStart 組件");
        }
        else
        {
            // 如果反射失敗，使用 SerializedObject
            var serializedObject = new SerializedObject(fontFixer);
            var fontProperty = serializedObject.FindProperty("chineseFontAsset");
            if (fontProperty != null)
            {
                fontProperty.objectReferenceValue = fontAsset;
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"✓ 已設置字體引用（使用 SerializedObject）");
            }
        }

        // 保存場景
        if (fontFixer.gameObject.scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fontFixer.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(fontFixer.gameObject.scene);
        }

        EditorUtility.DisplayDialog("完成", "字體引用已設置！請在 Inspector 中確認 FontFixerOnStart 組件的 chineseFontAsset 字段已正確引用字體。", "確定");
    }
}

