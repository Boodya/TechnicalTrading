using OoplesFinance.StockIndicators.Models;
using System.Collections.Generic;

namespace TechnicalTradingBot.Model
{
    public class CandlesUpdateModel
    {
        public List<TickerData> Candles { get; set; }
        public string Ticker { get; set; }
    }
}
