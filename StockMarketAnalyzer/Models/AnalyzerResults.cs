using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarketAnalyzer.Models
{
    public class AnalyzerResults
    {
        public List<Signal> Signals { get; set; }
        public Signal Decision { get; set; }
    }
}
