using System;

namespace JustCSharp.Utility.Extensions
{
    public static class DateTimeExtensions
    {
        public static readonly TimeZoneInfo VietNamTimezone = TimeZoneInfo.CreateCustomTimeZone("Asia/Hanoi", new TimeSpan(7,0,0), "Viet Nam Time", "Viet Nam Time");
        
        public static DateTime ToVietNamTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                throw new ArgumentException("Need to be UTC Time");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietNamTimezone);
        }
        
        public static DateTime FromVietNamTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Utc)
            {
                throw new ArgumentException("Need to be Local Time");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietNamTimezone);
        }

        public static DateTime ZeroOut(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Local)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, kind);
        }
    }
}
