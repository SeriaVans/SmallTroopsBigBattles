using UnityEngine;

namespace SmallTroopsBigBattles.Core.Utils
{
    /// <summary>
    /// 數學工具類
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// 隨機布林值
        /// </summary>
        public static bool RandomBool()
        {
            return Random.value > 0.5f;
        }

        /// <summary>
        /// 基於機率返回布林值
        /// </summary>
        public static bool RandomChance(float probability)
        {
            return Random.value < probability;
        }

        /// <summary>
        /// 範圍內隨機整數 (包含最大值)
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return Random.Range(min, max + 1);
        }

        /// <summary>
        /// 帶權重的隨機選擇
        /// </summary>
        public static int WeightedRandom(float[] weights)
        {
            float total = 0;
            foreach (var w in weights) total += w;

            float random = Random.value * total;
            float cumulative = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (random <= cumulative)
                    return i;
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// 計算百分比
        /// </summary>
        public static float Percentage(float current, float max)
        {
            if (max <= 0) return 0;
            return Mathf.Clamp01(current / max) * 100f;
        }

        /// <summary>
        /// 格式化大數字 (1000 -> 1K)
        /// </summary>
        public static string FormatNumber(long number)
        {
            if (number >= 1000000000)
                return (number / 1000000000f).ToString("0.#") + "B";
            if (number >= 1000000)
                return (number / 1000000f).ToString("0.#") + "M";
            if (number >= 1000)
                return (number / 1000f).ToString("0.#") + "K";
            return number.ToString();
        }

        /// <summary>
        /// 格式化大數字 (中文: 1000 -> 1千)
        /// </summary>
        public static string FormatNumberChinese(long number)
        {
            if (number >= 100000000)
                return (number / 100000000f).ToString("0.#") + "億";
            if (number >= 10000)
                return (number / 10000f).ToString("0.#") + "萬";
            if (number >= 1000)
                return (number / 1000f).ToString("0.#") + "千";
            return number.ToString();
        }

        /// <summary>
        /// 線性插值 (不限制範圍)
        /// </summary>
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// 平滑步進
        /// </summary>
        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(from, to, t);
        }

        /// <summary>
        /// 計算兩點間的曼哈頓距離
        /// </summary>
        public static float ManhattanDistance(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 將角度限制在 -180 到 180 之間
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
    }
}
