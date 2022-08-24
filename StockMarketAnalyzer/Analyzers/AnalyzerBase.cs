using OoplesFinance.StockIndicators.Models;
using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketAnalyzer.Analyzers
{
    public abstract class AnalyzerBase
    {
        protected AnalyzerResults ProceedThroughAnalyzing(IEnumerable<TickerData> tickers, Func<StockData, StockData> operation)
        {
            var stockData = new StockData(tickers);
            var calculated = operation.Invoke(stockData);
            return new AnalyzerResults()
            {
                Signals = calculated.SignalsList,
                Decision = calculated.SignalsList.LastOrDefault()
            };
        }
    }
}
