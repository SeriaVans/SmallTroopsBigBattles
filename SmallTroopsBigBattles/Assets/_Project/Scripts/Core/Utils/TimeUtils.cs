using System;
using UnityEngine;

namespace SmallTroopsBigBattles.Core.Utils
{
    public static class TimeUtils
    {
        public static string FormatTime(float seconds)
        {
            if (seconds <= 0) return "00:00:00";
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }

        public static string FormatTimeShort(float seconds)
        {
            if (seconds <= 0) return "00:00";
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{minutes:D2}:{secs:D2}";
        }

        public static string FormatTimeFriendly(float seconds)
        {
            if (seconds <= 0) return "完成";
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            if (hours > 0) return $"{hours}小時{minutes}分";
            if (minutes > 0) return $"{minutes}分{secs}秒";
            return $"{secs}秒";
        }

        public static long GetUnixTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static DateTime FromUnixTimestamp(long ts) => DateTimeOffset.FromUnixTimeSeconds(ts).DateTime;
        public static bool IsSameDay(DateTime a, DateTime b) => a.Year == b.Year && a.DayOfYear == b.DayOfYear;
        public static bool IsToday(DateTime date) => IsSameDay(date, DateTime.Now);
    }
}
