using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// UI 顯示檢查器 - 在運行時檢查 UI 是否正確顯示
    /// </summary>
    public class UIDisplayChecker : MonoBehaviour
    {
        [ContextMenu("檢查 UI 顯示狀態")]
        public void CheckUIDisplay()
        {
            Debug.Log("=== UI 顯示狀態檢查 ===");

            // 檢查 Canvas
            var canvas = GameObject.Find("MainCanvas");
            if (canvas != null)
            {
                Debug.Log($"✓ MainCanvas 存在");
                Debug.Log($"  - 激活: {canvas.activeSelf}");
                Debug.Log($"  - 在 Hierarchy 中激活: {canvas.activeInHierarchy}");

                var canvasComponent = canvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    Debug.Log($"  - Render Mode: {canvasComponent.renderMode}");
                    Debug.Log($"  - Enabled: {canvasComponent.enabled}");
                }
            }
            else
            {
                Debug.LogError("✗ MainCanvas 不存在！");
                return;
            }

            // 檢查 HUD
            var hud = GameObject.Find("MainCanvas/HUD");
            if (hud != null)
            {
                Debug.Log($"✓ HUD 存在");
                Debug.Log($"  - 激活: {hud.activeSelf}");
                Debug.Log($"  - 在 Hierarchy 中激活: {hud.activeInHierarchy}");

                var gameHUD = hud.GetComponent<UI.GameHUD>();
                if (gameHUD != null)
                {
                    Debug.Log($"  - GameHUD 組件存在");
                }
                else
                {
                    Debug.LogWarning("  - GameHUD 組件不存在！");
                }
            }
            else
            {
                Debug.LogError("✗ HUD 不存在！");
            }

            // 檢查頂部資源列
            CheckUIElement("MainCanvas/HUD/TopResourceBar", "頂部資源列");
            CheckTextElement("MainCanvas/HUD/TopResourceBar/CopperText", "銅錢文字");
            CheckTextElement("MainCanvas/HUD/TopResourceBar/WoodText", "木材文字");
            CheckTextElement("MainCanvas/HUD/TopResourceBar/StoneText", "石頭文字");
            CheckTextElement("MainCanvas/HUD/TopResourceBar/FoodText", "糧草文字");
            CheckTextElement("MainCanvas/HUD/TopResourceBar/SoldierCountText", "兵力文字");

            // 檢查底部按鈕列
            CheckUIElement("MainCanvas/HUD/BottomButtonBar", "底部按鈕列");
            CheckButton("MainCanvas/HUD/BottomButtonBar/TerritoryButton", "領地按鈕");
            CheckButton("MainCanvas/HUD/BottomButtonBar/ArmyButton", "軍隊按鈕");
            CheckButton("MainCanvas/HUD/BottomButtonBar/GeneralButton", "將領按鈕");
            CheckButton("MainCanvas/HUD/BottomButtonBar/MapButton", "地圖按鈕");
            CheckButton("MainCanvas/HUD/BottomButtonBar/SettingsButton", "設定按鈕");
            CheckButton("MainCanvas/HUD/BottomButtonBar/TestBattleButton", "測試戰鬥按鈕");

            // 檢查玩家資訊面板
            CheckUIElement("MainCanvas/HUD/PlayerInfoPanel", "玩家資訊面板");
            CheckTextElement("MainCanvas/HUD/PlayerInfoPanel/PlayerNameText", "玩家名稱");
            CheckTextElement("MainCanvas/HUD/PlayerInfoPanel/PlayerLevelText", "玩家等級");

            Debug.Log("====================");
        }

        private void CheckUIElement(string path, string name)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                var rect = obj.GetComponent<RectTransform>();
                var image = obj.GetComponent<Image>();
                Debug.Log($"✓ {name} 存在");
                Debug.Log($"  - 激活: {obj.activeSelf}");
                Debug.Log($"  - 在 Hierarchy 中激活: {obj.activeInHierarchy}");
                if (rect != null)
                {
                    Debug.Log($"  - 位置: {rect.anchoredPosition}");
                    Debug.Log($"  - 大小: {rect.sizeDelta}");
                    Debug.Log($"  - 錨點: min={rect.anchorMin}, max={rect.anchorMax}");
                }
                if (image != null)
                {
                    Debug.Log($"  - Image 顏色: {image.color}");
                    Debug.Log($"  - Image Enabled: {image.enabled}");
                }
            }
            else
            {
                Debug.LogError($"✗ {name} 不存在: {path}");
            }
        }

        private void CheckTextElement(string path, string name)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                var text = obj.GetComponent<TextMeshProUGUI>();
                Debug.Log($"✓ {name} 存在");
                Debug.Log($"  - 激活: {obj.activeSelf}");
                if (text != null)
                {
                    Debug.Log($"  - 文字: {text.text}");
                    Debug.Log($"  - 顏色: {text.color}");
                    Debug.Log($"  - Enabled: {text.enabled}");
                }
            }
            else
            {
                Debug.LogError($"✗ {name} 不存在: {path}");
            }
        }

        private void CheckButton(string path, string name)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                var button = obj.GetComponent<Button>();
                var image = obj.GetComponent<Image>();
                Debug.Log($"✓ {name} 存在");
                Debug.Log($"  - 激活: {obj.activeSelf}");
                if (button != null)
                {
                    Debug.Log($"  - Button Enabled: {button.enabled}");
                    Debug.Log($"  - Button Interactable: {button.interactable}");
                }
                if (image != null)
                {
                    Debug.Log($"  - Image 顏色: {image.color}");
                }
            }
            else
            {
                Debug.LogError($"✗ {name} 不存在: {path}");
            }
        }

        private void Start()
        {
            // 延遲檢查，確保所有組件都已初始化
            Invoke(nameof(CheckUIDisplay), 0.5f);
        }
    }
}

