using UnityEngine;

namespace SmallTroopsBigBattles.Core.Utils
{
    public static class MathUtils
    {
        public static bool RandomBool() => Random.value > 0.5f;
        public static bool RandomChance(float probability) => Random.value < probability;
        public static int RandomRange(int min, int max) => Random.Range(min, max + 1);

        public static int WeightedRandom(float[] weights)
        {
            float total = 0;
            foreach (var w in weights) total += w;
            float random = Random.value * total;
            float cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (random <= cumulative) return i;
            }
            return weights.Length - 1;
        }

        public static float Percentage(float current, float max) => max <= 0 ? 0 : Mathf.Clamp01(current / max) * 100f;

        public static string FormatNumber(long number)
        {
            if (number >= 1000000000) return (number / 1000000000f).ToString("0.#") + "B";
            if (number >= 1000000) return (number / 1000000f).ToString("0.#") + "M";
            if (number >= 1000) return (number / 1000f).ToString("0.#") + "K";
            return number.ToString();
        }

        public static string FormatNumberChinese(long number)
        {
            if (number >= 100000000) return (number / 100000000f).ToString("0.#") + "億";
            if (number >= 10000) return (number / 10000f).ToString("0.#") + "萬";
            if (number >= 1000) return (number / 1000f).ToString("0.#") + "千";
            return number.ToString();
        }
    }
}
