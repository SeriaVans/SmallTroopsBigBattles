using UnityEngine;
using TMPro;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// 中文字體應用器 - 在運行時自動應用中文字體到所有 UI 元素
    /// </summary>
    public class ChineseFontApplier : MonoBehaviour
    {
        [SerializeField] private bool applyOnStart = true;

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyChineseFont();
            }
        }

        [ContextMenu("應用中文字體")]
        public void ApplyChineseFont()
        {
            TMP_FontAsset fontAsset = null;

            // 嘗試加載中文字體
            #if UNITY_EDITOR
            fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/ChineseFont_SDF.asset");
            #endif

            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Fonts/ChineseFont_SDF");
            }

            // 如果還是沒有，嘗試使用默認字體但記錄警告
            if (fontAsset == null)
            {
                Debug.LogWarning("[ChineseFontApplier] ⚠ 未找到中文字體資源。請在編輯器中執行 Tools > SLG Game > 快速修復繁體中文顯示");
                
                // 嘗試使用 LiberationSans 作為臨時解決方案
                var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont != null)
                {
                    Debug.Log("[ChineseFontApplier] 使用默認字體（可能不支持中文）");
                }
                return;
            }

            // 應用字體到所有文字元素
            var canvas = GameObject.Find("MainCanvas");
            if (canvas != null)
            {
                var allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                int count = 0;
                foreach (var text in allTexts)
                {
                    if (text != null)
                    {
                        text.font = fontAsset;
                        count++;
                    }
                }
                Debug.Log($"[ChineseFontApplier] ✓ 已應用中文字體到 {count} 個文字元素");
            }
            else
            {
                Debug.LogWarning("[ChineseFontApplier] 找不到 MainCanvas");
            }
        }
    }
}

