using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Calculations;
using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarketAnalyzer.Analyzers
{
    class Calculate1LCLeastSquaresMovingAverage : AnalyzerBase, IMarketAnalyzer
    {
        public AnalyzerResults Analyze(IEnumerable<TickerData> tickers)
        {
            return base.ProceedThroughAnalyzing(tickers, (stockData) =>
            {
                return stockData.Calculate1LCLeastSquaresMovingAverage();
            });
        }
    }
}
