using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// UI 調試助手 - 幫助確認 UI 是否正確顯示
    /// </summary>
    public class UIDebugHelper : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== UI 調試資訊 ===");
            
            // 檢查 Canvas
            var canvas = GameObject.Find("MainCanvas");
            if (canvas != null)
            {
                Debug.Log($"✓ MainCanvas 存在，激活狀態: {canvas.activeSelf}");
                
                var canvasComponent = canvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    Debug.Log($"  - Render Mode: {canvasComponent.renderMode}");
                    Debug.Log($"  - Sorting Order: {canvasComponent.sortingOrder}");
                }
            }
            else
            {
                Debug.LogError("✗ MainCanvas 不存在！");
            }

            // 檢查 HUD
            var hud = GameObject.Find("MainCanvas/HUD");
            if (hud != null)
            {
                Debug.Log($"✓ HUD 存在，激活狀態: {hud.activeSelf}");
                
                // 檢查子元素
                CheckUIElement(hud.transform, "TopResourceBar");
                CheckUIElement(hud.transform, "BottomButtonBar");
                CheckUIElement(hud.transform, "PlayerInfoPanel");
            }
            else
            {
                Debug.LogError("✗ HUD 不存在！");
            }

            // 檢查按鈕
            var testButton = GameObject.Find("MainCanvas/HUD/BottomButtonBar/TestBattleButton");
            if (testButton != null)
            {
                Debug.Log($"✓ 測試戰鬥按鈕存在，激活狀態: {testButton.activeSelf}");
                
                var button = testButton.GetComponent<Button>();
                var image = testButton.GetComponent<Image>();
                var text = testButton.GetComponentInChildren<TextMeshProUGUI>();
                
                Debug.Log($"  - Button 組件: {(button != null ? "✓" : "✗")}");
                Debug.Log($"  - Image 組件: {(image != null ? "✓" : "✗")}, 顏色: {image?.color}");
                Debug.Log($"  - Text 組件: {(text != null ? "✓" : "✗")}, 文字: {text?.text}");
            }
            else
            {
                Debug.LogError("✗ 測試戰鬥按鈕不存在！");
            }

            Debug.Log("===================");
        }

        private void CheckUIElement(Transform parent, string name)
        {
            var element = parent.Find(name);
            if (element != null)
            {
                Debug.Log($"  ✓ {name} 存在，激活狀態: {element.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning($"  ✗ {name} 不存在！");
            }
        }
    }
}

