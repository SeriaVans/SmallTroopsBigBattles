using System;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Utils
{
    /// <summary>
    /// 時間工具類
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// 格式化時間為 HH:MM:SS
        /// </summary>
        public static string FormatTime(float seconds)
        {
            if (seconds <= 0) return "00:00:00";

            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);

            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// 格式化時間為 MM:SS (短格式)
        /// </summary>
        public static string FormatTimeShort(float seconds)
        {
            if (seconds <= 0) return "00:00";

            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);

            return $"{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// 格式化為友善時間 (如: 2小時30分)
        /// </summary>
        public static string FormatTimeFriendly(float seconds)
        {
            if (seconds <= 0) return "完成";

            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);

            if (hours > 0)
                return $"{hours}小時{minutes}分";
            if (minutes > 0)
                return $"{minutes}分{secs}秒";
            return $"{secs}秒";
        }

        /// <summary>
        /// 取得 Unix 時間戳
        /// </summary>
        public static long GetUnixTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 從 Unix 時間戳轉換
        /// </summary>
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        /// <summary>
        /// 判斷是否為同一天
        /// </summary>
        public static bool IsSameDay(DateTime a, DateTime b)
        {
            return a.Year == b.Year && a.DayOfYear == b.DayOfYear;
        }

        /// <summary>
        /// 判斷是否為今天
        /// </summary>
        public static bool IsToday(DateTime date)
        {
            return IsSameDay(date, DateTime.Now);
        }

        /// <summary>
        /// 取得距離今天結束的秒數
        /// </summary>
        public static float GetSecondsUntilMidnight()
        {
            var now = DateTime.Now;
            var midnight = now.Date.AddDays(1);
            return (float)(midnight - now).TotalSeconds;
        }

        /// <summary>
        /// 取得距離本週結束的秒數
        /// </summary>
        public static float GetSecondsUntilWeekEnd()
        {
            var now = DateTime.Now;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;

            var weekEnd = now.Date.AddDays(daysUntilMonday);
            return (float)(weekEnd - now).TotalSeconds;
        }
    }
}
