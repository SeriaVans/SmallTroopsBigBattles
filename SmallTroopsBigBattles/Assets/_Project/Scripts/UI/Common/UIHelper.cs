using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// UI 輔助工具
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 安全設置文字 (支援 Text 和 TMP)
        /// </summary>
        public static void SetText(GameObject obj, string text)
        {
            if (obj == null) return;

            var tmpText = obj.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = text;
                return;
            }

            var uiText = obj.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.text = text;
            }
        }

        /// <summary>
        /// 安全設置文字 (Component 版本)
        /// </summary>
        public static void SetText(Component component, string text)
        {
            if (component == null) return;
            SetText(component.gameObject, text);
        }

        /// <summary>
        /// 設置按鈕啟用狀態
        /// </summary>
        public static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button == null) return;
            button.interactable = enabled;
        }

        /// <summary>
        /// 設置圖片填充量
        /// </summary>
        public static void SetFillAmount(Image image, float amount)
        {
            if (image == null) return;
            image.fillAmount = Mathf.Clamp01(amount);
        }

        /// <summary>
        /// 設置圖片顏色
        /// </summary>
        public static void SetImageColor(Image image, Color color)
        {
            if (image == null) return;
            image.color = color;
        }

        /// <summary>
        /// 設置圖片精靈
        /// </summary>
        public static void SetSprite(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
        }

        /// <summary>
        /// 設置物件啟用狀態
        /// </summary>
        public static void SetActive(GameObject obj, bool active)
        {
            if (obj == null) return;
            if (obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }

        /// <summary>
        /// 設置物件啟用狀態 (Component 版本)
        /// </summary>
        public static void SetActive(Component component, bool active)
        {
            if (component == null) return;
            SetActive(component.gameObject, active);
        }

        /// <summary>
        /// 清空容器內所有子物件
        /// </summary>
        public static void ClearChildren(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 取得或添加元件
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 格式化資源數量文字
        /// </summary>
        public static string FormatResource(int amount, int max = -1)
        {
            if (max > 0)
            {
                return $"{amount:N0}/{max:N0}";
            }
            return amount.ToString("N0");
        }

        /// <summary>
        /// 格式化百分比
        /// </summary>
        public static string FormatPercent(float value)
        {
            return $"{value:P0}";
        }

        /// <summary>
        /// 根據數值設定顏色 (紅/黃/綠)
        /// </summary>
        public static Color GetValueColor(float current, float max)
        {
            float ratio = current / max;
            if (ratio > 0.6f) return Color.green;
            if (ratio > 0.3f) return Color.yellow;
            return Color.red;
        }
    }
}
