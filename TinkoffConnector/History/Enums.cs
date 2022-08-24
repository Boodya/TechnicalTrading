using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.History
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
    
    public enum CandleIntervals
    {
        Minute,
        TwoMinutes,
        ThreeMinutes,
        FiveMinutes,
        TenMinutes,
        QuarterHour,
        HalfHour,
        Hour,
        Day,
        Week,
        Month
    }
}
