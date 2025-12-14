using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    public static class UIHelper
    {
        public static void SetText(GameObject obj, string text)
        {
            if (obj == null) return;
            var tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp != null) { tmp.text = text; return; }
            var ui = obj.GetComponent<Text>();
            if (ui != null) ui.text = text;
        }

        public static void SetText(Component c, string text) { if (c != null) SetText(c.gameObject, text); }
        public static void SetButtonEnabled(Button b, bool enabled) { if (b != null) b.interactable = enabled; }
        public static void SetFillAmount(Image img, float amount) { if (img != null) img.fillAmount = Mathf.Clamp01(amount); }
        public static void SetImageColor(Image img, Color color) { if (img != null) img.color = color; }
        public static void SetSprite(Image img, Sprite sprite) { if (img != null) img.sprite = sprite; }

        public static void SetActive(GameObject obj, bool active)
        {
            if (obj != null && obj.activeSelf != active) obj.SetActive(active);
        }

        public static void SetActive(Component c, bool active) { if (c != null) SetActive(c.gameObject, active); }

        public static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.Destroy(parent.GetChild(i).gameObject);
        }

        public static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            var c = obj.GetComponent<T>();
            return c ?? obj.AddComponent<T>();
        }

        public static string FormatResource(int amount, int max = -1) =>
            max > 0 ? $"{amount:N0}/{max:N0}" : amount.ToString("N0");

        public static string FormatPercent(float value) => $"{value:P0}";

        public static Color GetValueColor(float current, float max)
        {
            float ratio = current / max;
            if (ratio > 0.6f) return Color.green;
            if (ratio > 0.3f) return Color.yellow;
            return Color.red;
        }
    }
}
