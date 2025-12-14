using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// 啟動時自動修復字體問題 - 確保字體在運行時正確加載
    /// </summary>
    public class FontFixerOnStart : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset chineseFontAsset; // 在 Inspector 中直接引用字體
        
        private void Awake()
        {
            // 在 Awake 中立即嘗試加載字體，避免在 Start 中太晚
            LoadAndApplyFont();
        }

        private void Start()
        {
            // 延遲再次應用，確保所有 UI 都已初始化
            Invoke(nameof(LoadAndApplyFont), 0.2f);
        }

        private void LoadAndApplyFont()
        {
            TMP_FontAsset fontAsset = null;

            // 優先使用 Inspector 中直接引用的字體
            if (chineseFontAsset != null)
            {
                fontAsset = chineseFontAsset;
                Debug.Log("[FontFixerOnStart] 使用 Inspector 中引用的字體");
            }
            else
            {
                #if UNITY_EDITOR
                // 在編輯器中，嘗試從 Assets 路徑加載
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/ChineseFont_SDF.asset");
                if (fontAsset != null)
                {
                    Debug.Log("[FontFixerOnStart] 從 Assets 路徑加載字體（編輯器模式）");
                }
                #endif

                // 運行時，嘗試從 Resources 加載
                if (fontAsset == null)
                {
                    fontAsset = Resources.Load<TMP_FontAsset>("Fonts/ChineseFont_SDF");
                    if (fontAsset != null)
                    {
                        Debug.Log("[FontFixerOnStart] 從 Resources 加載字體");
                    }
                }

                // 如果還是沒有，嘗試使用 GUID 加載
                if (fontAsset == null)
                {
                    #if UNITY_EDITOR
                    fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/ChineseFont_SDF.asset");
                    #endif
                }
            }

            if (fontAsset == null)
            {
                Debug.LogWarning("[FontFixerOnStart] ⚠ 未找到中文字體資源！請確保字體資源存在於 Assets/_Project/Fonts/ChineseFont_SDF.asset");
                return;
            }

            // 檢查字體資源是否有效
            if (fontAsset.characterTable == null || fontAsset.characterTable.Count == 0)
            {
                Debug.LogWarning("[FontFixerOnStart] ⚠ 字體資源無效（字符表為空）");
                return;
            }

            // 確保字體資源不會被銷毀
            DontDestroyOnLoad(fontAsset);

            // 應用字體
            ApplyFont(fontAsset);
        }

        private void ApplyFont(TMP_FontAsset font)
        {
            if (font == null) return;

            var canvas = GameObject.Find("MainCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[FontFixerOnStart] 找不到 MainCanvas");
                return;
            }

            var allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            int count = 0;
            int errorCount = 0;

            foreach (var text in allTexts)
            {
                if (text != null)
                {
                    try
                    {
                        // 強制設置字體，即使已經設置過
                        text.font = font;
                        
                        // 強制重建文字
                        text.ForceMeshUpdate();
                        
                        count++;
                    }
                    catch (System.Exception e)
                    {
                        errorCount++;
                        Debug.LogWarning($"[FontFixerOnStart] 應用字體到 {text.name} 時出錯: {e.Message}");
                    }
                }
            }

            Debug.Log($"[FontFixerOnStart] ✓ 已應用中文字體到 {count} 個文字元素" + (errorCount > 0 ? $"，{errorCount} 個失敗" : ""));
        }
    }
}
