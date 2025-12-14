using UnityEngine;
using TMPro;
using System.Collections;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// UI 運行時修復器 - 在遊戲運行時自動修復 UI 顯示和應用中文字體
    /// </summary>
    public class UIRuntimeFixer : MonoBehaviour
    {
        [SerializeField] private bool autoFixOnStart = true;
        [SerializeField] private bool applyChineseFont = true;

        private void Start()
        {
            if (autoFixOnStart)
            {
                StartCoroutine(FixUIAfterFrame());
            }
        }

        private IEnumerator FixUIAfterFrame()
        {
            // 等待一幀，確保所有組件都已初始化
            yield return null;

            Debug.Log("[UIRuntimeFixer] 開始修復 UI...");

            // 1. 確保 Canvas 正確設置
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
                Debug.Log("[UIRuntimeFixer] ✓ Canvas 已修復");
            }

            // 2. 確保 HUD 可見
            var hud = GameObject.Find("MainCanvas/HUD");
            if (hud != null)
            {
                hud.SetActive(true);
                EnsureAllChildrenActive(hud);
                Debug.Log("[UIRuntimeFixer] ✓ HUD 已激活");
            }

            // 3. 應用中文字體
            if (applyChineseFont)
            {
                yield return StartCoroutine(ApplyChineseFontCoroutine());
            }

            Debug.Log("[UIRuntimeFixer] UI 修復完成！");
        }

        private void EnsureAllChildrenActive(GameObject obj)
        {
            obj.SetActive(true);
            foreach (Transform child in obj.transform)
            {
                EnsureAllChildrenActive(child.gameObject);
            }
        }

        private IEnumerator ApplyChineseFontCoroutine()
        {
            // 嘗試加載中文字體
            var fontAsset = Resources.Load<TMP_FontAsset>("Fonts/ChineseFont_SDF");
            
            // 如果 Resources 中沒有，嘗試使用默認字體
            // 注意：運行時無法使用 AssetDatabase，需要在編輯器中預先設置

            // 如果還是沒有，使用默認字體但記錄警告
            if (fontAsset == null)
            {
                Debug.LogWarning("[UIRuntimeFixer] ⚠ 未找到中文字體資源，將使用默認字體");
                yield break;
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
                Debug.Log($"[UIRuntimeFixer] ✓ 已應用中文字體到 {count} 個文字元素");
            }
        }

        [ContextMenu("手動修復 UI")]
        public void ManualFixUI()
        {
            StartCoroutine(FixUIAfterFrame());
        }
    }
}

