using System;

namespace JustCSharp.Utility.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ZeroOut(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Local)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, kind);
        }
    }
}
