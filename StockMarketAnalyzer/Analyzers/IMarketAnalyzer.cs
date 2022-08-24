using OoplesFinance.StockIndicators.Models;
using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarketAnalyzer.Analyzers
{
    public interface IMarketAnalyzer
    {
        public AnalyzerResults Analyze(IEnumerable<TickerData> tickers);
    }
}
