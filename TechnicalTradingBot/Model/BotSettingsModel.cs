using System;
using System.Collections.Generic;
using System.Text;
using TechnicalTradingBot.Providers;

namespace TechnicalTradingBot.Model
{
    public class BotSettingsModel
    {
        public IExecutionOrchestrator ExecutionOrchestrator { get; set; }
        public ICandleProvider CandlesProvider { get; set; }
        public IStockMarketExecutionProvider StockMarketExecutionProvider { get; set; }
        /// <summary>
        /// Amount of Time when full Indicators analyzis should be performed
        /// </summary>
        public TimeSpan RecalculationPeriod { get; set; }
        public bool IsShortAvailable { get; set; }
        public string StockHistoryDatabasePath { get; set; }
        public Currency Currency { get; set; }
        public int InstrumentsAmountToTrade { get; set; }
    }
}
