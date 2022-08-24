using OoplesFinance.StockIndicators.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarketAnalyzer.Helpers
{
    static class AnalyzerHelpers
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        public static Dictionary<DateTime, Signal> MergeSignals(IEnumerable<Dictionary<DateTime, Signal>> signals)
        {
            throw new NotImplementedException();
        }
    }
}
