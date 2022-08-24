using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot.Extensions
{
    public static class GlobalExtensions
    {
        public static DateTime GetCopy(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day,
                dt.Hour, dt.Minute, dt.Second);
        }
    }
}
